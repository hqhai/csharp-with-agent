// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class SectionTimeCodeDtoModel
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public double DisplayTime { get; set; }
        public double ExecutionTime { get; set; }
        public Guid SectionId { get; set; }
        public object? Answer { get; set; }
    }
}
