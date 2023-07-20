// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class StudentReviewInfoModel : BaseModel
    {
        public EnumReviewType ReviewType { get; set; }
        public Guid StudentId { get; set; }
        public Guid? CourseId { get; set; }
        public string? CourseName { get; set; }
        public IList<StudentReviewDetailModel>? StudentReviewDetails { get; set; }
    }
}
