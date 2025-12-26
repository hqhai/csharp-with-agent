// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.V1i2
{
    using Enums;

    public class DocumentResultModel
    {
        public EnumResultStatus Status { get; set; }
        public Guid StudentId { get; set; }
        public Guid DocumentId { get; set; }
        public Guid LessonModuleId { get; set; }
        public Guid LessonResultId { get; set; }
    }
}
