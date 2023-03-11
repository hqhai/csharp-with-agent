using Fsel.Core.Base;
using Fsel.Core.Base.Interfaces;
using MediatR;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fsel.Core.Entities
{
    public class Entity : ValidationEntity, IEntity
    {
        [Key]
        [Column(Order = 0)]
        public virtual Guid Id { get; set; }

        [Column(Order = 101)]
        public Guid CreatedUserId { get; set; }

        [Column(Order = 102)]
        public Guid? UpdatedUserId { get; set; }

        [Column(Order = 103)]
        public Guid? DeletedUserId { get; set; }

        [Column(Order = 104)]
        [MaxLength(100)]
        public string CreatedFullName { get; set; }

        [Column(Order = 105)]
        [MaxLength(100)]
        public string? UpdatedFullName { get; set; }

        [Column(Order = 106)]
        [MaxLength(100)]
        public string? DeletedFullName { get; set; }

        [Column(Order = 107)]
        public DateTime CreatedDate { get; set; }

        [Column(Order = 108)]
        public DateTime? UpdatedDate { get; set; }

        [Column(Order = 109)]
        public DateTime? DeletedDate { get; set; }

        [Column(Order = 110)]
        [DefaultValue("false")]
        public bool IsDeleted { get; set; }

        private int? _requestedHashCode;

        private List<INotification>? _domainEvents;

        public IReadOnlyCollection<INotification> DomainEvents
        {
            get
            {
                if (_domainEvents != null)
                    return _domainEvents.AsReadOnly();
                return new List<INotification>();
            }
        }

        protected Entity()
        {
            CreatedDate = DateTime.Now;
            CreatedUserId = Guid.Empty;
            CreatedFullName = string.Empty;
            Id = Guid.Empty;
        }

        public void AddDomainEvent(INotification eventItem)
        {
            _domainEvents = _domainEvents ?? new List<INotification>();
            _domainEvents.Add(eventItem);
        }

        public void RemoveDomainEvent(INotification eventItem)
        {
            _domainEvents?.Remove(eventItem);
        }

        public void ClearDomainEvents()
        {
            _domainEvents?.Clear();
        }

        public bool IsTransient()
        {
            return Id == Guid.Empty;
        }

        public override bool Equals(object? obj)
        {
            if (obj == null || !(obj is Entity))
            {
                return false;
            }

            if (this == obj)
            {
                return true;
            }

            if (GetType() != obj.GetType())
            {
                return false;
            }

            Entity entity = (Entity)obj;
            if (entity.IsTransient() || IsTransient())
            {
                return false;
            }

            return entity.Id == Id;
        }

        public override int GetHashCode()
        {
            if (!IsTransient())
            {
                if (!_requestedHashCode.HasValue)
                {
                    _requestedHashCode = Id.GetHashCode() ^ 0x1F;
                }

                return _requestedHashCode.Value;
            }

            return base.GetHashCode();
        }
    }
}