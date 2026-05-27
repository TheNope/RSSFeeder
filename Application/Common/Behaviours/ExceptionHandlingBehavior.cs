using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Common.Behaviours;

public class ExceptionHandlingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<ExceptionHandlingBehavior<TRequest, TResponse>> _logger;

    public ExceptionHandlingBehavior(ILogger<ExceptionHandlingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            // Execute the next behavior in the pipeline
            return await next();
        }
        catch (Exception ex)
        {
            // Log the exception
            _logger.LogError(ex, "An exception occurred during request execution.");

            // Throw a custom exception or return a default response as needed
            // Return the exception details as an HttpResult
            var result = new HttpResult(ex);
            return (TResponse)(object)result;
        }
    }
}