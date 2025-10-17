// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;

    public class TopicModel : BaseModel
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public long Usage { get; set; }
        public int CountHomeWork { get; set; }
    }
}
