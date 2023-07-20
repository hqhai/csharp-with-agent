// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.CommandModels.StudentReviews
{
    using Fsel.Interaction.Domain.Models.CommandModels.StudentReviewDetails;
    using Fsel.Shared.Enums;

    public class SaveStudentReviewCommandModel
    {
        public Guid? Id { get; set; }
        public EnumReviewType ReviewType { get; set; }
        public IList<SaveStudentReviewDetailCommandModel>? StudentReviewDetails { get; set; }
    }
}
