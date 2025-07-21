// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System.Text.Json.Serialization;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Entities;

    public class ClassForumDetailResultHistoryModel : BaseModel
    {
        public string? Content { get; set; }

        public string? WordContent { get; set; }

        public Guid ClassForumDetailResultId { get; set; }

        [JsonIgnore]
        public IList<ClassForumResultFile>? ClassForumResultFiles { get; set; }

        public IList<string>? FilePaths
        { get { return ClassForumResultFiles?.Select(x => x.FilePath ?? string.Empty).ToList(); } }
    }
}
