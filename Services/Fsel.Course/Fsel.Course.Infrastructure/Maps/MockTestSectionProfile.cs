// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Maps
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Models.EntityModels;

    public class MockTestSectionProfile : Profile
    {
        public MockTestSectionProfile()
        {
            CreateMap<MockTestSection, MockTestSectionModel>().IgnoreAllNonExisting();
        }
    }
}
