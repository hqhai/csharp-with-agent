using Fsel.Core.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Core.Applications.InternalEvents
{
    public class EntityChangedEvent<T> : INotification where T : Entity
    {
        public T Data { get; set; }

        public EntityChangedEvent(T entity)
        {
            Data = entity;
        }
    }
}
