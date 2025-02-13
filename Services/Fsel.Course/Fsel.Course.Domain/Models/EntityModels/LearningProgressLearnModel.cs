// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class LearningProgressLearnModel
    {
        public Guid? StudentId { get; set; }
        public long? DisplayOrder { get; set; }
        public long? UnitDisplayOrder { get; set; }
        public long StudentCount { get; set; }
    }
}
