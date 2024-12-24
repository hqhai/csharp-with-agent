// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.LmsCourseService.Model
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class CourseResultModel : BaseModel
    {
        public EnumCourseType? CourseType { get; set; }
        public EnumWorkingStatus WorkingStatus { get; set; }
        public EnumCourseLevel? CourseLevel { get; set; }
        public DateTime? ProcessDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public Guid CourseId { get; set; }
    }
}
