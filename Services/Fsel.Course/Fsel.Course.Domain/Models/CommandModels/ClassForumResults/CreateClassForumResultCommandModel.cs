// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.ClassForumResults
{
    public class CreateClassForumResultCommandModel
    {
        public string? Content { get; set; }

        public Guid LessonResultId { get; set; }

        public IList<string>? FilePaths { get; set; }

        public bool IsSubmit { get; set; }
    }
}
