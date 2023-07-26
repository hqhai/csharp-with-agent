// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class ReviewCommunicationSearchModel
    {
        public double Scores { get; set; }
        public PagingItemsModel<ReviewCommunicationModel> PagingItemsModel { get; set; } = new PagingItemsModel<ReviewCommunicationModel>();
    }

    public class ReviewCommunicationModel : BaseModel
    {
        public EnumReviewType ReviewType { get; set; }
        public Guid StudentId { get; set; }
        public Guid? CourseId { get; set; }
        public string? Code { get; set; }
        public string? CodeStudent { get; set; }
        public EnumCourseLevel? CourseLevel { get; set; }
        public double Scores { get; set; }
        public IList<ReviewCommunicationStudentModel>? StudentReviewDetails { get; set; }
    }

    public class ReviewCommunicationStudentModel
    {
        public Guid Id { get; set; }
        public EnumReviewQuestionType ReviewQuestionType { get; set; }
        public int VoteStars { get; set; }
        public string? Content { get; set; }
    }
}
