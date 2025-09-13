// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class CurriculumModel : BaseModel
    {
        public string? CurriculumName { get; set; }
        public string? CourseName { get; set; }
        public string? Subject { get; set; }
        public Guid CourseId { get; set; }
        public Guid CourseCloneId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public EnumCourseType CourseType { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public int NumberOfStudent { get; set; }
    }
}
