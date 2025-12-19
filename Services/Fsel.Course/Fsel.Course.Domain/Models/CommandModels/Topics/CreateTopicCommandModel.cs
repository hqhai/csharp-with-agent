// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.Topics
{
    public class CreateTopicCommandModel
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public long Usage { get; set; }
    }
}
