using Fsel.Core.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Core.Base.Interfaces
{
    public interface IEntity
    {
        Guid CreatedUserId { get; set; }

        Guid? UpdatedUserId { get; set; }

        Guid? DeletedUserId { get; set; }

        string CreatedUserName { get; set; }

        string? UpdatedUserName { get; set; }

        string? DeletedUserName { get; set; }

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
