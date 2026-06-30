using System;
using System.Collections.Frozen;
using System.Threading;
using System.Threading.Tasks;

namespace CQRS.Interfaces;

internal sealed class Mediator(
    IServiceProvider provider,
    DispatcherRegistry registry) : IMediator
{
    public Task<TResponse> Send<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!registry.RequestWrappers.TryGetValue(request.GetType(), out var wrapper))
        {
            throw new InvalidOperationException(
                $"No handler registered for request type '{request.GetType().FullName}'.");
        }

        // Reference-type cast - cheap, no boxing.
        return ((RequestHandlerBase<TResponse>)wrapper).Handle(request, provider, cancellationToken);
    }

    // ... Publish for INotification omitted, see the repo
}

internal sealed class DispatcherRegistry(
    FrozenDictionary<Type, RequestHandlerBase> requestWrappers
)
{
    public FrozenDictionary<Type, RequestHandlerBase> RequestWrappers { get; } = requestWrappers;
}