// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.SectionTimeCodes
{
    public class UpdateSectionTimeCodeCommandModel
    {
        public string? Name { get; set; }
        public long DisplayTime { get; set; }
        public long ExecutionTime { get; set; }
        public Guid SectionId { get; set; }
    }
}
