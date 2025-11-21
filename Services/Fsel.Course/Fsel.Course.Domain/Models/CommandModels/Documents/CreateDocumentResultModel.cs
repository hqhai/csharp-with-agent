// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.Documents
{
    using Enums;

    public class CreateDocumentResultModel
    {
        public EnumResultStatus Status { get; set; }
        public Guid StudentId { get; set; }
        public Guid DocumentId { get; set; }
        public Guid LessonModuleId { get; set; }
        public Guid LessonResultId { get; set; }
    }
}
