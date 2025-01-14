using Application.Automapper;
using Application.Dtos.In;
using Application.Dtos.In.Validators;
using Application.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Application;

public static class ServiceCollectionExtension
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddAutoMapper(typeof(ApplicationProfile));
        builder.Services.AddScoped<IValidator<CreateUserBetRequest>, CreateUserBetRequestValidator>();
        builder.Services.AddTransient<IBetService, BetService>();
        builder.Services.AddTransient<IStatsService, StatsService>();
    }
}