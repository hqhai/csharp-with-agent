using MediatR;

namespace Fsel.Core.Base.Interfaces
{
    public interface IEntity
    {
        Guid CreatedUserId { get; set; }

        Guid? UpdatedUserId { get; set; }

        Guid? DeletedUserId { get; set; }

        string CreatedFullName { get; set; }

        string? UpdatedFullName { get; set; }

        string? DeletedFullName { get; set; }

        DateTime CreatedDate { get; set; }

        DateTime? UpdatedDate { get; set; }

        DateTime? DeletedDate { get; set; }

        bool IsDeleted { get; set; }

        public IReadOnlyCollection<INotification> DomainEvents { get; }

        void AddDomainEvent(INotification eventItem);

        void RemoveDomainEvent(INotification eventItem);

        void ClearDomainEvents();

        bool IsTransient();

        bool Equals(object? obj);

        int GetHashCode();
    }
}