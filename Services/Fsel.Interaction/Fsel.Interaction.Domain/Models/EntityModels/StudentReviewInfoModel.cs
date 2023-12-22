// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;

    public class StudentReviewInfoModel
    {
        public Guid Id { get; set; }
        public EnumReviewType ReviewType { get; set; }
        public Guid StudentId { get; set; }
        public Guid? CourseId { get; set; }
        public string? CourseName { get; set; }
        public double VoteStars { get; set; }
        public IList<StudentReviewDetailModel>? StudentReviewDetails { get; set; }
    }
}
