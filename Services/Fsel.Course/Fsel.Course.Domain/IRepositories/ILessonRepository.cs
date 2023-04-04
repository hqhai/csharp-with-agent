// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base.Interfaces;
using Fsel.Course.Domain.Entities;

namespace Fsel.Course.Domain.IRepositories
{
    public interface ILessonRepository : IRepository<Lesson>
    {
        Task<bool> IsLessonUsed(Guid id);

        Task<Lesson?> GetIncludeVideoByIdAsync(Guid id);
    }
}
