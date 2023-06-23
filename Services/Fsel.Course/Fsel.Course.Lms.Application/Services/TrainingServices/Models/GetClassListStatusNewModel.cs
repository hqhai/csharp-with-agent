// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.TrainingServices.Models
{
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;

    public class GetClassListStatusNewModel
    {
        public IList<CourseClassModel>? Courses { get; set; }
        public EnumCourseLevel? CourseLevel { get; set; }
        public Guid PackageId { get; set; }
        public Guid? LiveTimeFrameId { get; set; }
        public IList<DayOfWeek>? LiveDays { get; set; }

    }
}
