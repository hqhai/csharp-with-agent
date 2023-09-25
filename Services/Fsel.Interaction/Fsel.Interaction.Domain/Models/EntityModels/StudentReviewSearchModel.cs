// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class StudentReviewSearchModel
    {
        public double Stars { get; set; }
        public string? Code { get; set; }
        public PagingItemsModel<StudentReviewTypeModel> PagingItems { get; set; } = new PagingItemsModel<StudentReviewTypeModel>();
    }

    public class StudentReviewTypeModel : BaseModel
    {
        public EnumReviewType ReviewType { get; set; }
        public Guid StudentId { get; set; }
        public Guid? CourseId { get; set; }
        public EnumCourseLevel? CourseLevel { get; set; }
        public double Stars { get; set; }
        public string? Code { get; set; }
        public string? CodeStudent { get; set; }
        public string? ClassCode { get; set; }
        public IList<StudentReviewQuestionTypeModel>? StudentReviewQuestionTypes { get; set; }
    }

    public class StudentReviewQuestionTypeModel
    {
        public Guid Id { get; set; }
        public EnumReviewQuestionType ReviewQuestionType { get; set; }
        public double VoteStars { get; set; }
        public string? Content { get; set; }
    }
}
