// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.CommandModels.StudentReviews
{
    using System;
    using System.Collections.Generic;
    using Fsel.Interaction.Domain.Enums;
    using Fsel.Interaction.Domain.Models.CommandModels.StudentReviewDetails;

    public class CreateStudentReviewCommandModel
    {
        public EnumReviewType ReviewType { get; set; }
        public Guid? CourseId { get; set; }
        public IList<CreateStudentReviewDetailCommandModel>? StudentReviewDetails { get; set; }
    }
}
