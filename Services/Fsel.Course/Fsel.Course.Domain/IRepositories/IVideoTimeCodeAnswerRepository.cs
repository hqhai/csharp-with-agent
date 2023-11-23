// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base.Interfaces;
using Fsel.Course.Domain.Entities;

namespace Fsel.Course.Domain.IRepositories
{
    public interface IVideoTimeCodeAnswerRepository : IRepository<VideoTimeCodeAnswer>
    {
        Task<VideoTimeCodeAnswer?> GetAsync(Guid videoTimeCodeId, Guid videoResultId, Guid questionId, Guid? exerciseId);
    }
}
