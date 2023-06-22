// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;

    public class SectionTimeCodeModel
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public double DisplayTime { get; set; }
        public double ExecutionTime { get; set; }
        public Guid SectionId { get; set; }
    }
}
