// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Models.CommandModels.ClassForumResults;
    using Fsel.Course.Domain.Models.EntityModels;

    public class StudentFeedbackProfile : Profile
    {
        public StudentFeedbackProfile()
        {
            CreateMap<StudentFeedback, StudentFeedbackModel>().IgnoreAllNonExisting();
            
            CreateMap<RateClassForumResultCommandModel, StudentFeedback>().IgnoreAllNonExisting();
        }
    }
}
