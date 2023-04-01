// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Entities;
using MediatR;

namespace Fsel.Core.Applications.InternalEvents
{
    public class EntityCreatedEvent<T> : INotification where T : Entity
    {
        public T Data { get; set; }

        public EntityCreatedEvent(T entity)
        {
            Data = entity;
        }
    }
}
