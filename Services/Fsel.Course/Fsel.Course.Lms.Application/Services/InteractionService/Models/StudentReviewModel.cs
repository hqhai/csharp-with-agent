// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.InteractionService.Models
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class StudentReviewModel : BaseModel
    {
        public EnumReviewType ReviewType { get; set; }
        public Guid StudentId { get; set; }
        public Guid? CourseId { get; set; }
        public EnumCourseLevel? CourseLevel { get; set; }
        public string? Code { get; set; }
        public IList<StudentReviewDetailModel>? StudentReviewDetails { get; set; }
    }
}
