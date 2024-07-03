// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities
{
    using Fsel.Core.Entities;

    public class StudentTechie : Entity
    {
        public string? Config { get; set; }

        public string? Message { get; set; }

        public TechieAction? TechieAction { get; set; }

        public Guid TechieActionId { get; set; }

        public Guid StudentId { get; set; }

    }
}
