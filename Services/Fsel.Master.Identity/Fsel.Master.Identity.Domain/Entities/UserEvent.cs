// Copyright (c) Atlantic. All rights reserved.

using System.ComponentModel.DataAnnotations.Schema;
using Fsel.Common.Helpers;
using Fsel.Core.Entities;

namespace Fsel.Master.Identity.Domain.Entities
{
    public class UserEvent : Entity
    {
        public Guid UserId { get; set; }

        public string? EventCode { get; set; }

        public string? SchoolIdsStr { get; set; }

        [NotMapped]
        public IList<Guid>? SchoolIds
        {
            get
            {
                return ConvertHelper.Deserialize<IList<Guid>>(SchoolIdsStr);
            }
            set
            {
                SchoolIdsStr = ConvertHelper.Serialize(value);
            }
        }
    }
}
