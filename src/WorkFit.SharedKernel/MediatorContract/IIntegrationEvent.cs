
namespace WorkFit.SharedKernel.MediatorContract;

public interface IIntegrationEvent
{
}

public interface IIntegrationEventHandler<in TIntegrationEvent> where TIntegrationEvent : IIntegrationEvent
{
    Task Handle(TIntegrationEvent @event, CancellationToken cancellationToken = default);
}


