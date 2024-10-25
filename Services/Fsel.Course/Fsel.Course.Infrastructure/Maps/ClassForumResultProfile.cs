// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Models.CommandModels.ClassForumResults;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.ClassForumAutoDot;
    using Fsel.Shared.Models.ShareModels;

    public class ClassForumResultProfile : Profile
    {
        public ClassForumResultProfile()
        {
            CreateMap<ClassForumResult, ClassForumResultModel>()
                .ForMember(p => p.ClassForumScores, x => x.MapFrom(n => n.ClassForumScores.OrderBy(x => x.CreatedDate)));

            CreateMap<CreateClassForumResultCommandModel, ClassForumResult>().IgnoreAllNonExisting();
            CreateMap<ClassForumAIResponseModel, SetTimeRetryClassForumModel>().IgnoreAllNonExisting();
            CreateMap<RateClassForumResultCommandModel, ClassForumResult>().IgnoreAllNonExisting();
            CreateMap<RetryClassForumResultCommandModel, ClassForumResult>().IgnoreAllNonExisting();
            CreateMap<ClassForumResult, ClassForumReportModel>().IgnoreAllNonExisting();
            CreateMap<ClassForum, ClassForumReportModel>().IgnoreAllNonExisting();
            CreateMap<ClassForumResult, ClassForumResultInfoModel>()
                .ForMember(p => p.UnitId, x => x.MapFrom(n => n.LessonResult!.UnitId))
                .ForMember(p => p.CourseId, x => x.MapFrom(n => n.LessonResult!.CourseId));
        }
    }
}
