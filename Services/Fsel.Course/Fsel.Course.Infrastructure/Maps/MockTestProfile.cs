// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Models.CommandModels.MockTestAnswers;
    using Fsel.Course.Domain.Models.CommandModels.MockTestResults;
    using Fsel.Course.Domain.Models.CommandModels.MockTests;
    using Fsel.Course.Domain.Models.EntityModels;

    public class MockTestProfile : Profile
    {
        public MockTestProfile()
        {
            CreateMap<MockTest, MockTestModel>().IgnoreAllNonExisting();
            CreateMap<CreateMockTestCommandModel, MockTest>().IgnoreAllNonExisting();
            CreateMap<UpdateMockTestCommandModel, MockTest>().IgnoreAllNonExisting();

            CreateMap<CreateMockTestAnswerCommandModel, MockTestAnswer>().IgnoreAllNonExisting();
            CreateMap<MockTestScore, MockTestScoreModel>().IgnoreAllNonExisting();
            CreateMap<CreateMockTestScoreCommandModel, MockTestScore>().IgnoreAllNonExisting();
            CreateMap<MockTest, MockTestModel>()
                 .ForMember(x => x.IsActive, p => p.MapFrom(o => o.CourseUnitMockTests.Any()));
        }
    }
}
