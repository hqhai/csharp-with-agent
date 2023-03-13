// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Maps
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Fsel.Core.Extensions;
    using AutoMapper;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Models.CommandModels.PlacementTests;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.CommandModels.CourseUnitMockTests;
    using Fsel.Course.Domain.Models.CommandModels.MockTests;

    public class MockTestProfile : Profile
    {
        public MockTestProfile()
        {
            CreateMap<MockTest, MockTestModel>().IgnoreAllNonExisting();
            CreateMap<CreateMockTestCommandModel, MockTest>().IgnoreAllNonExisting();
            CreateMap<UpdateMockTestCommandModel, MockTest>().IgnoreAllNonExisting();
        }
    }
}
