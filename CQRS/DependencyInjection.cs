using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Reflection;
using CQRS.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace CQRS;

public static class DependencyInjection
{
    public static IServiceCollection AddCQRS(this IServiceCollection services, Assembly assembly)
    {
        services.AddMediator(assembly);
        //services.AddPipelineBehavior(typeof(IRequestHandler<,>));
        return services;
    }
    
    public static IServiceCollection AddMediator(this IServiceCollection services, Assembly assembly)
    {
        var requestWrappers = new Dictionary<Type, RequestHandlerBase>();

        foreach (var type in assembly.GetTypes())
        {
            if (type.IsAbstract || type.IsInterface) 
                continue;

            foreach (var iface in type.GetInterfaces())
            {
                if (!iface.IsGenericType) 
                    continue;
                
                var def = iface.GetGenericTypeDefinition();

                if (def == typeof(IRequestHandler<,>))
                {
                    services.AddScoped(iface, type);

                    var args = iface.GetGenericArguments();
                    var requestType = args[0];
                    var responseType = args[1];

                    if (!requestWrappers.ContainsKey(requestType))
                    {
                        var wrapperType = typeof(RequestHandlerWrapper<,>)
                            .MakeGenericType(requestType, responseType);
                        requestWrappers[requestType] =
                            (RequestHandlerBase)Activator.CreateInstance(wrapperType)!;
                    }
                }
            }
        }

        var registry = new DispatcherRegistry(
            requestWrappers.ToFrozenDictionary());

        services.AddSingleton(registry);
        services.AddScoped<Mediator>();
        services.AddScoped<IMediator>(sp => sp.GetRequiredService<Mediator>());

        return services;
    }
    
    public static IServiceCollection AddPipelineBehavior(this IServiceCollection services, Type openGenericBehaviorType)
    {
        services.AddScoped(typeof(IPipelineBehavior<,>), openGenericBehaviorType);
        return services;
    }
}