
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Infrastructure.MediatorImp;

internal sealed class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;

    public Mediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task Send(IRequest request, CancellationToken cancellationToken = default)
    {
        var handlerType = typeof(IRequestHandler<>).MakeGenericType(request.GetType());
        var handler = _serviceProvider.GetService(handlerType);
        if (handler == null)
        {
            throw new InvalidOperationException($"Handler for request type {request.GetType().Name} not found.");
        }

        var method = handlerType.GetMethod("Handle");
        if (method == null)
        {
            throw new InvalidOperationException($"Handle method not found in handler for request type {request.GetType().Name}.");
        }

        return (Task)method.Invoke(handler, new object[] { request, cancellationToken });
    }

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));
        var handler = _serviceProvider.GetService(handlerType);
        if (handler == null)
        {
            throw new InvalidOperationException($"Handler for request type {request.GetType().Name} not found.");
        }

        var method = handlerType.GetMethod("Handle");
        if (method == null)
        {
            throw new InvalidOperationException($"Handle method not found in handler for request type {request.GetType().Name}.");
        }

        return (Task<TResponse>)method.Invoke(handler, new object[] { request, cancellationToken });
    }
}
