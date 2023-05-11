// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;
    using Fsel.Core.Base.BaseModels;

    public class SectionTimeCodeModel : BaseModel
    {
        public string? Name { get; set; }
        public long DisplayTime { get; set; }
        public long ExecutionTime { get; set; }
        public Guid SectionId { get; set; }
    }
}
