// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.SystemService.Models
{
    using System;
    using Fsel.Core.Base.BaseModels;

    public class SchoolModel : BaseModel
    {
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }

        public string? Description { get; set; }

        public string? PrincipalName { get; set; }

        public string? PrincipalPhone { get; set; }

        public string? PrincipalEmail { get; set; }

        public string? Website { get; set; }

        public string? IdPath { get; set; }

        public string? LocationName { get; set; }

        public string? LongPath { get; set; }

        public string? ShortPath { get; set; }
        public Guid? LocationId { get; set; }

        public bool IsActive { get; set; }

        public Location? Location { get; set; }
    }

    public class Location
    {
        public string? LongPath { get; set; }
    }
}
