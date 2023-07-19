// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.CommandModels.StudentReviews
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Interaction.Domain.Models.CommandModels.StudentReviewDetails;

    public class UpdateStudentReviewCommandModel : BaseCommandModel
    {
        public IList<UpdateStudentReviewDetailCommandModel>? StudentReviewDetails { get; set; }
    }
}
