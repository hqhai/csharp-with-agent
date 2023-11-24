// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.Ais
{
    using Fsel.Course.Domain.Entities;

    public class SubmitAICommandModel
    {
        public string? WordContent { get; set; }

        public ClassForum? ClassForum { get; set; }
    }
}
