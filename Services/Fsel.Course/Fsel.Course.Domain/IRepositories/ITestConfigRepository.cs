// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.IRepositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.TestConfig;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.EntityModels.TestConfig;

    public interface ITestConfigRepository : IRepository<TestConfig>
    {
    }
}
