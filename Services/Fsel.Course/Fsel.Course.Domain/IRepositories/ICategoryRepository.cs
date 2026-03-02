// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.IRepositories
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Course.Domain.Entities;

    public interface ICategoryRepository : IRepository<Category>
    {
        Task<Category?> GetProgramLevelsAsync(Guid? programId, CancellationToken cancellationToken);

        Task<Category?> GetSecondLevelFromRootAsync(Guid? programId, CancellationToken cancellationToken);
    }
}
