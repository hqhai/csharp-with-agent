// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.IRepositories
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Course.Domain.Entities;

    public interface ISkillRepository : IRepository<Skill>
    {
        Task<bool> IsDuplicateFieldValueAsync(string? fieldName, string? value);

        Task<bool> IsDuplicateFieldValueAsync(Guid id, string? fieldName, string? value);
    }
}
