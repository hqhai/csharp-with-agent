// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base.Interfaces;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Entities.V1i1;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Shared.Enums;

namespace Fsel.Course.Domain.IRepositories
{
    public interface IVideoRepository : IRepository<Video>
    {
        Task<bool> IsVideoUsed(Guid? id);

        Task<VideoModel?> GetIncludeAllAsync(Guid? id);

        Task<double> GetPercent(Guid courseId, Guid unitId, Guid? studentId);

        IQueryable<VideoSearchModel> SearchAsync(EnumTimeCodeType? codeType, Guid? teacherId, EnumCourseLevel? courseLevel);

        Task<IDictionary<Guid, Video>> GetVideoDicAsync(IList<Guid>? originalIds);

        Task<(IDictionary<Guid, (Video, VideoResult)>, IDictionary<Guid, Video>)> BuildVideoLookupsAsync(LessonResult lessonResult, IList<LessonModule> lessonModules);
    }
}
