// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities
{
    using Fsel.Core.Entities;
    using Fsel.System.Domain.Enums;
    using global::System;

    public class School : Entity
    {
        public string? Name { get; set; }
        public EnumEducationLevel EducationLevel { get; set; }
        public EnumSchoolType SchoolType { get; set; }
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
        public Location? Location { get; set; }
    }
}
