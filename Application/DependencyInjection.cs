using System.Reflection;
using Application.Common.Behaviours;
using CQRS;
using CQRS.Interfaces;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Register Mediator
            services.AddMediator(Assembly.GetExecutingAssembly());
            
            // Register Validator
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            
            // Register pipeline behaviors
            services.AddPipelineBehavior(typeof(ExceptionHandlingBehavior<,>));

            return services;
        }
    }
}