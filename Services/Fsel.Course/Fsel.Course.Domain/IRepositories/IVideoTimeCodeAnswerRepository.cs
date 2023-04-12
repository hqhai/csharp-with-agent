// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base.Interfaces;
using Fsel.Course.Domain.Entities;

namespace Fsel.Course.Domain.IRepositories
{
    public interface IVideoTimeCodeAnswerRepository : IRepository<VideoTimeCodeAnswer>
    {
        Task<bool> AnyAsync(Guid videoResultId, Guid questionId, Guid? exerciseId, Guid? videoTimeCodeId);
    }
}
