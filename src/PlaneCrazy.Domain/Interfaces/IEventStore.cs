using PlaneCrazy.Domain.Events;

namespace PlaneCrazy.Domain.Interfaces;

public interface IEventStore
{
    Task AppendAsync(DomainEvent domainEvent);
    Task<IEnumerable<DomainEvent>> GetAllAsync();
    Task<IEnumerable<DomainEvent>> GetByTypeAsync(string eventType);
}
