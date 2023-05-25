// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;
    using Fsel.Core.Base.BaseModels;

    public class ClassForumFileModel : BaseModel
    {
        public string? FilePath { get; set; }

        public Guid ClassForumId { get; set; }
        public ClassForumModel? ClassForum { get; set; }
    }
}
