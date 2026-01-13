// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base.Interfaces;
using Fsel.Course.Domain.Entities;

namespace Fsel.Course.Domain.IRepositories
{
    public interface IVideoResultRepository : IRepository<VideoResult>
    {
        Task<VideoResult?> GetIncludeTimeCodeAnswerByIdAsync(Guid videoId, Guid lessonResultId, Guid studentId);

        Task<List<VideoResult>> GetVideoResultsAsync(Guid courseId, Guid studentId);
    }
}
