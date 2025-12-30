// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.IRepositories
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Course.Domain.Entities;

    public interface ISkillLevelRepository : IRepository<SkillLevel>
    {
        Task<IList<Skill>> GetDefaultSkillsByProgramIdAsync(Guid? programId);
    }
}
