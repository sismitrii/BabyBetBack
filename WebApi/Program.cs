using BabyBetBack.Auth;
using BabyBetBack.Configuration;
using DAL;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;
using Application;
using Core.Entities;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);
DotNetEnv.Env.Load();

if (!builder.Environment.IsDevelopment())
{
    var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
    builder.WebHost.UseUrls("http://*:" + port);
}


builder.Services.AddHealthChecks();

builder.Services.AddControllers().AddJsonOptions(x =>
{
    // serialize enums as strings in api responses (e.g. Role)
    x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularLocalhost",
        policy =>
        {
            // DO NOT TOUCH Except for deployment
            policy.WithOrigins("https://baby-bet.vercel.app")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
 options.SwaggerDoc("v1", new OpenApiInfo
 {
     Title = "Mon API",
     Version = "v1"
 });

 // Ajouter la configuration pour le Bearer Token
 options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
 {
     Name = "Authorization",
     Type = SecuritySchemeType.ApiKey,
     Scheme = "Bearer",
     BearerFormat = "JWT",
     In = ParameterLocation.Header,
     Description = "Entrez 'Bearer' suivi d'un espace et du token JWT.\n\nExemple : Bearer xxxxxx.yyyyy.zzzzz"
 });

 options.AddSecurityRequirement(new OpenApiSecurityRequirement
 {
     {
         new OpenApiSecurityScheme
         {
             Reference = new OpenApiReference
             {
                 Type = ReferenceType.SecurityScheme,
                 Id = "Bearer"
             }
         },
         new string[] {"Bearer"}
     }
 });
});

builder.Services.AddIdentity<User, Role>(options =>
    {
        options.Password.RequiredLength = 8;

        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
        options.Lockout.MaxFailedAccessAttempts = 3;
        options.User.RequireUniqueEmail = true;
    }).AddEntityFrameworkStores<BetDbContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
{
    options.TokenLifespan = TimeSpan.FromHours(24);
});

builder.Services.AddTransient<DbContext, BetDbContext>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IGoogleAuthService, GoogleAuthService>();

builder.AddApplicationServices();
builder.AddInfrastructureServices();

builder.Services.Configure<GoogleAuthConfig>(builder.Configuration.GetSection("Google"));

var jwtSection = builder.Configuration.GetSection("JWT");
builder.Services.Configure<JwtConfiguration>(jwtSection);
var jwtConfig = jwtSection.Get<JwtConfiguration>();
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET");
var secret = Encoding.ASCII.GetBytes(jwtSecret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

}).AddJwtBearer(o =>
{
    o.RequireHttpsMetadata = true;
    o.SaveToken = true;
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtConfig.ValidIssuer,
        ValidAudience = jwtConfig.ValidAudience,
        ValidateIssuerSigningKey = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        RequireExpirationTime = true,
        IssuerSigningKey = new SymmetricSecurityKey(secret)
    };

});

var connectionString = builder.Configuration.GetConnectionString("Database");
builder.Services.AddDbContext<BetDbContext>(opt =>
    opt.UseSqlite(connectionString));

var app = builder.Build();

app.UseHealthChecks("/health");

app.UseCors("AllowAngularLocalhost");
app.UseAuthentication();
app.UseAuthorization();


// Configure the HTTP request pipeline.
    app.UseSwagger();
    app.UseSwaggerUI();

app.UseHttpsRedirection();



app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BetDbContext>();
    Console.WriteLine(db.Database.GetAppliedMigrationsAsync());
    db.Database.Migrate(); // Applique les migrations si nécessaire
}
app.Run();

