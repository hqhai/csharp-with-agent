// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Domain.Entities
{
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Cms.PlanetDefender.Domain.Enums;
    using Fsel.Core.Entities;

    public class NewsAndUpdate : Entity
    {
        public string? Titile { get; set; }

        public DateTime StartDate { get; set; }

        [NotMapped]
        public bool IsActive
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

        public string? Description { get; set; }

        public string? FilePath { get; set; }

        public EnumNewsAndUpdateType Type { get; set; }
    }
}
