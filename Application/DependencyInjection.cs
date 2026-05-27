using System;
using System.Reflection;
using Application.Common.Behaviours;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Register MediatR
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));
            
            // Register AutoMapper
            services.AddAutoMapper(cfg => { }, Assembly.GetExecutingAssembly());
            
            // Register Validator
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            
            // Register pipeline behaviors
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ExceptionHandlingBehavior<,>));

            return services;
        }
    }
}