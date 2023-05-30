// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Models.CommandModels.ClassForumScores;
    using Fsel.Course.Domain.Models.EntityModels;

    public class ClassForumScoreProfile : Profile
    {
        public ClassForumScoreProfile()
        {
            CreateMap<ClassForumScore, ClassForumScoreModel>().IgnoreAllNonExisting();
            CreateMap<CreateClassForumScoreCommandModel, ClassForumScore>().IgnoreAllNonExisting();
            CreateMap<CreateListClassForumscoreCommandModel, ClassForumScore>().IgnoreAllNonExisting();
        }
    }
}
