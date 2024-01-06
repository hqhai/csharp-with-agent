// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.ClassForumResults
{
    using Fsel.Core.Base.BaseModels;

    public class RetryClassForumResultCommandModel : BaseCommandModel
    {
        public string? Content { get; set; }

        public string? WordContent { get; set; }

        public IList<string>? FilePaths { get; set; }
    }
}
