// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.IRepositories
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Course.Domain.Entities;
    using Fsel.Shared.Enums;

    public interface ISectionGroupRepository : IRepository<SectionGroup>
    {
        Task<SectionGroup?> GetAsync(Guid id, Guid? objectResultId, string? type, EnumCourseSkill? enumCourseSkill);
    }
}
