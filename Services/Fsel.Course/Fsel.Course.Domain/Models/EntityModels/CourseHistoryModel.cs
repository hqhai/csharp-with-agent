// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;

    public class CourseHistoryModel : BaseModel
    {
        public string? Name { get; set; }

        public string? Code { get; set; }

        public int Version { get; set; }
    }
}
