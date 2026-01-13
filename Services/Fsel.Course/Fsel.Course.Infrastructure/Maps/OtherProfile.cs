// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Entities.BandScoresConfigs;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.EntityModels.ManagerReportModels;
    using Fsel.Course.Domain.Models.QueryModels.ClassForumAutoDot;
    using Fsel.Shared.Models.ShareModels;

    public class OtherProfile : Profile
    {
        public OtherProfile()
        {
            CreateMap<BandScores, BandScoresReport>().IgnoreAllNonExisting();
            CreateMap<OverallReportPlacementTestModel, SearchReportPlacementTestModel>().IgnoreAllNonExisting();
            CreateMap<OverallReportLearningProgressModel, SearchReportLearningProgressModel>().IgnoreAllNonExisting();
            CreateMap<OverallReportLearningResultModel, SearchReportLearningResultModel>().IgnoreAllNonExisting();
            CreateMap<CourseCompleteReportModel, CourseCompleteModel>().IgnoreAllNonExisting();
            CreateMap<MockTestAnswerResponseModel, SetTimeRetryMockTestModel>().IgnoreAllNonExisting();
        }
    }
}
