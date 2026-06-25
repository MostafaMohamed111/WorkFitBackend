
namespace WorkFit.SharedKernel.MediatorContract;

public interface IMediator
{
    Task Send(IRequest request, CancellationToken cancellationToken = default);
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
    Task Publish(IIntegrationEvent @event, CancellationToken cancellationToken = default);
}
