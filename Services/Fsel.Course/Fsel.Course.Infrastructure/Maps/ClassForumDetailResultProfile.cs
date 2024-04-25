// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Models.CommandModels.ClassForumResults;
    using Fsel.Course.Domain.Models.EntityModels;

    public class ClassForumDetailResultProfile : Profile
    {
        public ClassForumDetailResultProfile()
        {
            CreateMap<ClassForumDetailResult, ClassForumDetailResultModel>().IgnoreAllNonExisting();
            CreateMap<CreateClassForumResultCommandModel, ClassForumDetailResult>().IgnoreAllNonExisting();
        }
    }
}
