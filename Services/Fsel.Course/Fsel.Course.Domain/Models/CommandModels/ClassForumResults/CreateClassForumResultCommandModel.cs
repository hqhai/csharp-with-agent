// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.ClassForumResults
{
    public class CreateClassForumResultCommandModel
    {
        public string? Content { get; set; }
        public Guid ClassForumResultId { get; set; }
        public IList<string>? FilePaths { get; set; }
        public bool IsSubmit { get; set; }
        public string? WordContent { get; set; }
        public Guid? ClassForumDetailResultId { get; set; }
    }
}
