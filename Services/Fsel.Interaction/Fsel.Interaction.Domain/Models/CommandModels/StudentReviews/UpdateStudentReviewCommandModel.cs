// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.CommandModels.StudentReviews
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Interaction.Domain.Enums;
    using Fsel.Interaction.Domain.Models.CommandModels.StudentReviewDetails;

    public class UpdateStudentReviewCommandModel : BaseCommandModel
    {
        public EnumReviewType ReviewType { get; set; }
        public Guid? CourseId { get; set; }
        public IList<CreateStudentReviewDetailCommandModel>? StudentReviewDetails { get; set; }
    }
}
