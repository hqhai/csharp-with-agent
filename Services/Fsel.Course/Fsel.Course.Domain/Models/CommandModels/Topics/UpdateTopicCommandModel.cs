// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.Topics
{
    using Fsel.Core.Base.BaseModels;

    public class UpdateTopicCommandModel : BaseCommandModel
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public long Usage { get; set; }
    }
}
