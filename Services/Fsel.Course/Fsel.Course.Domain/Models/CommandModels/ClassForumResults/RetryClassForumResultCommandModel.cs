// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.ClassForumResults
{
    using Fsel.Core.Base.BaseModels;

    public class RetryClassForumResultCommandModel : BaseCommandModel
    {
        public string? RetryContent { get; set; }

        public string? RetryWordContent { get; set; }

        public IList<string>? RetryFilePaths { get; set; }
    }
}
