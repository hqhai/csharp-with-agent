// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Interaction.Domain.Models.CommandModels.StudentReviewDetails;
    using Fsel.Interaction.Domain.Models.CommandModels.StudentReviews;
    using Fsel.Interaction.Domain.Models.EntityModels;

    public class StudentReviewProfile : Profile
    {
        public StudentReviewProfile()
        {
            CreateMap<StudentReview, StudentReviewModel>().IgnoreAllNonExisting();
            CreateMap<UpdateStudentReviewCommandModel, StudentReview>().IgnoreAllNonExisting();

            CreateMap<StudentReviewDetail, StudentReviewDetailModel>().IgnoreAllNonExisting();
            CreateMap<UpdateStudentReviewDetailCommandModel, StudentReviewDetail>().IgnoreAllNonExisting();
        }
    }
}
