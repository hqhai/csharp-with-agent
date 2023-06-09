// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;
    using Fsel.Core.Base.BaseModels;

    public class ClassModel : BaseModel
    {
        public string? Code { get; set; }

        public string? Name { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public EnumClassType Status { get; set; }

        public Guid CourseId { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }
    }
}
