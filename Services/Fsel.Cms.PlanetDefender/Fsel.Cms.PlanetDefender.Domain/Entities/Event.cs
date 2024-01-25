// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Cms.PlanetDefender.Domain.Enums;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;

    public class Event : Entity
    {
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Title { get; set; }

        public DateTime StartDate { get; set; }

        [NotMapped]
        public bool Status
        {
            get
            {
                DateTime now = DateTime.UtcNow;
                DateTime tomorrow = StartDate.AddDays(1);
                if (StartDate >= now && now <= tomorrow)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Description { get; set; }

        public string? FilePath { get; set; }

        public EnumEventType Type { get; set; }
    }
}
