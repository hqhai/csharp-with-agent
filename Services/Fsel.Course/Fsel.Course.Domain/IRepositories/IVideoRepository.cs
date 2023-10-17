// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base.Interfaces;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Shared.Enums;

namespace Fsel.Course.Domain.IRepositories
{
    public interface IVideoRepository : IRepository<Video>
    {
        Task<bool> IsVideoUsed(Guid? id);

        Task<VideoModel?> GetIncludeAllAsync(Guid? id);

        Task<double> GetPercent(Guid unitId, Guid? studentId);

        IQueryable<VideoSearchModel> SearchAsync(EnumTimeCodeType? codeType, Guid? teacherId, EnumCourseLevel? courseLevel);
    }
}
