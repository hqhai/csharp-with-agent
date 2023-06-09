// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;

    public class SectionTimeCodeModel
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public long DisplayTime { get; set; }
        public long ExecutionTime { get; set; }
        public Guid SectionId { get; set; }
    }
}
