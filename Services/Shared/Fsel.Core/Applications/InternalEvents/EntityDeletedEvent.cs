using Fsel.Core.Entities;
using MediatR;

namespace Fsel.Core.Applications.InternalEvents
{
    public class EntityDeletedEvent<T> : INotification where T : Entity
    {
        public T Data { get; set; }

        public EntityDeletedEvent(T entity)
        {
            Data = entity;
        }
    }
}