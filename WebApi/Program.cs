using DAL;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;
using Application;
using Application.Configuration;
using BabyBetBack.Middleware;
using Core.Entities;
using Microsoft.OpenApi.Models;
using NLog.Web;


var builder = WebApplication.CreateBuilder(args);
DotNetEnv.Env.Load();

if (!builder.Environment.IsDevelopment())
{
    var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
    builder.WebHost.UseUrls("http://*:" + port);
}

builder.Services.AddHealthChecks();
builder.Logging.ClearProviders();
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
builder.Host.UseNLog();

builder.Services.AddControllers().AddJsonOptions(x =>
{
    // serialize enums as strings in api responses (ex : Gender)
    x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

var corsOrigins = Environment.GetEnvironmentVariable("CORS_ORIGINS");
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularLocalhost",
        policy =>
        {
            // DO NOT TOUCH
            policy.WithOrigins(corsOrigins)
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
         ["Bearer"]
     }
 });
});

builder.Services.AddIdentity<User, Role>(options =>
    {
        options.SignIn.RequireConfirmedEmail = true; 
        options.Password.RequiredLength = 8;
        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
        options.Lockout.MaxFailedAccessAttempts = 3;
        options.User.RequireUniqueEmail = true;
    }).AddEntityFrameworkStores<BetDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthorization();

builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
{
    options.TokenLifespan = TimeSpan.FromHours(24);
});

builder.Services.AddTransient<DbContext, BetDbContext>();

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

app.UseMiddleware<ExceptionHandlingMiddleware>();

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
    
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    string[] roleNames = { "Admin", "User" };

    foreach (var roleName in roleNames)
    {
        var roleExists = await roleManager.RoleExistsAsync(roleName);
        if (!roleExists)
        {
            await roleManager.CreateAsync(new Role{Name = roleName});
        }
    }
    
    var adminUser = await userManager.FindByNameAsync("admin");
    if (adminUser == null)
    {
        adminUser = new User
        {
            UserName = "admin",
            FirstName = "admin",
            LastName = "admin",
            Email = "admin@oops.com",
            EmailConfirmed = true
            
        };
        var createResult = await userManager.CreateAsync(adminUser, "D0nTL3akPl$");
        if (createResult.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
        else
        {
            Console.WriteLine("Erreur lors de la création de l'utilisateur admin : " +
                              string.Join(", ", createResult.Errors.Select(e => e.Description)));
        }
    }
}

app.Run();

