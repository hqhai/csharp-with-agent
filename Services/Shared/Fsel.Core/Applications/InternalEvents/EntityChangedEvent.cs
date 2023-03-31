// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Entities;
using MediatR;

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
