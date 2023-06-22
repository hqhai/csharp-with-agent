// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.SectionTimeCodes
{
    public class UpdateSectionTimeCodeCommandModel
    {
        public string? Name { get; set; }
        public double DisplayTime { get; set; }
        public double ExecutionTime { get; set; }
        public Guid SectionId { get; set; }
    }
}
