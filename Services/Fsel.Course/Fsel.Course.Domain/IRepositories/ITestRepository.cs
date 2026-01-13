// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.IRepositories
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.Entities.V1i1;

    public interface ITestRepository : IRepository<Test>
    {
        Task<Test> GetTestAsync(Test test);

        Task<List<Guid>> GetUsedOriginalIdsAsync(IList<Guid> originalIds);

        Task<bool> IsUsingByClient(Guid originalId);

        Task<(IDictionary<Guid, (Test, TestGroupResult, TestResult)>, IDictionary<Guid, Test>)> BuildTestLookupsAsync(UnitResult unitResult, IList<UnitModule> unitModules);

        Task<(IDictionary<Guid, (Test, TestGroupResult, TestResult)>, IDictionary<Guid, Test>)> BuildTestLookupsAsync(CourseResult courseResult, IList<CourseModule> courseModules);
    }
}
