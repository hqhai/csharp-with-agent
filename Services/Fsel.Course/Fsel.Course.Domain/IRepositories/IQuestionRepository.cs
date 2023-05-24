// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base.Interfaces;
using Fsel.Course.Domain.Entities;

namespace Fsel.Course.Domain.IRepositories
{
    public interface IQuestionRepository : IRepository<Question>
    {
        Task<List<Question>?> GetIncludeTimeCodeByIdAsync(IEnumerable<Guid> ids);
        Task<List<Question>?> GetIncludeSectionByIdAsync(IEnumerable<Guid> ids);
        Task<List<Question>?> GetIncludeExerciseByIdAsync(IEnumerable<Guid> ids);
    }
}