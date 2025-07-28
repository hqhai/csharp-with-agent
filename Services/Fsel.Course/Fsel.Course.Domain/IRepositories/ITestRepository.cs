// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.IRepositories
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Course.Domain.Entities.TestConfigs;

    public interface ITestRepository : IRepository<Test>
    {
        Task<Test> GetTestAsync(Test test);

        Task<List<Guid>> GetUsedOriginalIdsAsync(IList<Guid> originalIds);

        Task<bool> IsUsingByClient(Guid originalId);
    }
}
