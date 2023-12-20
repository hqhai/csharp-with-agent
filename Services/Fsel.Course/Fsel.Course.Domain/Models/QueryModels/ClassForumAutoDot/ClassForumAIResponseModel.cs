// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.ClassForumAutoDot
{
    using Fsel.Course.Domain.Models.CommandModels.Ais;

    public class ClassForumAIResponseModel : SubmitAICommandModel
    {
        public Guid ClassForumResultId { get; set; }

        public string? WordContent { get; set; }


    }
}
