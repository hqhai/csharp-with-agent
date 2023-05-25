// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.ClassForumResults
{
    using Fsel.Course.Domain.Models.EntityModels;

    public class CreateClassForumResultCommandModel
    {
        public string? Content { get; set; }

        public Guid LessonResultId { get; set; }

        public Guid StudentId { get; set; }

        public Guid ClassForumId { get; set; }

        public IList<ClassForumResultFileModel>? ClassForumResultFiles { get; set; }
    }
}
