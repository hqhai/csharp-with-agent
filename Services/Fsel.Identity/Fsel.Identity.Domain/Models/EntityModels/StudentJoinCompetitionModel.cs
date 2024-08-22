// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    using System;
    public class StudentJoinCompetitionModel
    {
        public Guid StudentId { get; set; }

        public Guid UserId { get; set; }

        public string? FullName { get; set; }

        public string? SchoolName { get; set; }

        public double? Grade { get; set; }

        public string? Email { get; set; }

        public string? ParentEmail { get; set; }

    }
}
