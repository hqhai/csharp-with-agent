// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using System.Threading.Tasks;
    using Fsel.Common.Enums;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Shared.Helpers;
    using Microsoft.EntityFrameworkCore;

    public interface IProgressService
    {
        Task<List<(Guid UnitId, double Percent)>> GetUnitVideoPercentsAsync(Guid courseResultId, IReadOnlyCollection<Guid> unitIds, CancellationToken cancellationToken);

        Task<List<(Guid UnitId, double Percent)>> GetUnitHomeWorkPercentsAsync(Guid courseResultId, IReadOnlyCollection<Guid> unitIds, CancellationToken cancellationToken);

        Task<List<(Guid UnitId, double Percent)>> GetUnitClassForumPercentsAsync(Guid courseResultId, IReadOnlyCollection<Guid> unitIds, CancellationToken cancellationToken);

        Task<List<(Guid UnitId, double Percent)>> GetUnitVideoUnitTestPercentsAsync(Guid courseResultId, IReadOnlyCollection<Guid> unitIds, CancellationToken cancellationToken);
    }

    public class ProgressService : IProgressService
    {
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly IUnitModuleRepository _unitModuleRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly IHomeWorkResultRepository _homeWorkResultRepository;
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly IVideoRepository _videoRepository;
        private readonly IVideoTimeCodeResultRepository _videoTimeCodeResultRepository;
        private readonly IVideoTimeCodeRepository _videoTimeCodeRepository;
        private readonly ILessonModuleRepository _lessonModuleRepository;

        public ProgressService(IVideoResultRepository videoResultRepository,
            IUnitModuleRepository unitModuleRepository,
            ILessonResultRepository lessonResultRepository,
            ILessonRepository lessonRepository,
            IHomeWorkResultRepository homeWorkResultRepository,
            IClassForumResultRepository classForumResultRepository,
            IVideoRepository videoRepository,
            IVideoTimeCodeResultRepository videoTimeCodeResultRepository,
            IVideoTimeCodeRepository videoTimeCodeRepository,
            ILessonModuleRepository lessonModuleRepository)

        {
            _videoResultRepository = videoResultRepository;
            _unitModuleRepository = unitModuleRepository;
            _lessonResultRepository = lessonResultRepository;
            _lessonRepository = lessonRepository;
            _homeWorkResultRepository = homeWorkResultRepository;
            _classForumResultRepository = classForumResultRepository;
            _videoRepository = videoRepository;
            _videoTimeCodeResultRepository = videoTimeCodeResultRepository;
            _videoTimeCodeRepository = videoTimeCodeRepository;
            _lessonModuleRepository = lessonModuleRepository;
        }

        public async Task<List<(Guid UnitId, double Percent)>> GetUnitVideoPercentsAsync(Guid courseResultId,
        IReadOnlyCollection<Guid> unitIds,
        CancellationToken cancellationToken)
        {
            // 1) Done video theo Unit
            var doneByUnit = await
            (
                from lr in _lessonResultRepository.ReadQueryable
                join vr in _videoResultRepository.ReadQueryable on lr.Id equals vr.LessonResultId
                join vtcr in _videoTimeCodeResultRepository.ReadQueryable on vr.Id equals vtcr.VideoResultId
                join vtc in _videoTimeCodeRepository.ReadQueryable on vtcr.VideoTimeCodeId equals vtc.Id
                where lr.CourseResultId == courseResultId && vtcr.Status == EnumResultStatus.Done
                && vtc.TimeCodeType == EnumTimeCodeType.Standalone
                group vr.Id by lr.UnitId into g
                select new
                {
                    UnitId = g.Key,
                    DoneCount = g.Select(x => x).Distinct().Count()
                }
            ).ToListAsync(cancellationToken);

            // 2) Lessons cố định của Unit (LastVersion) theo OriginalId
            var moduleLessons = await
            (
                from um in _unitModuleRepository.ReadQueryable
                join l in _lessonRepository.ReadQueryable on um.OriginalId equals l.OriginalId
                where unitIds.Contains(um.UnitId) // ✅ đúng key
                      && um.UnitConfigType == EnumUnitConfigType.Lesson
                      && l.VersionStatus == EnumVersionStatus.LastVersion
                select new
                {
                    UnitId = um.UnitId,            // ✅ đúng UnitId
                    LessonOriginalId = l.OriginalId,
                    VideoCount = l.VideoCount
                }
            ).ToListAsync(cancellationToken);

            // 3) Lessons đã có Result (ưu tiên đúng version)
            var resultLessonMap = await
            (
                from lr in _lessonResultRepository.ReadQueryable
                join l in _lessonRepository.ReadQueryable on lr.LessonId equals l.Id
                where lr.CourseResultId == courseResultId
                select new
                {
                    UnitId = lr.UnitId,
                    LessonOriginalId = l.OriginalId,
                    VideoCount = l.VideoCount
                }
            ).ToListAsync(cancellationToken);

            var resultDict = resultLessonMap.ToDictionary(
                x => (x.UnitId, x.LessonOriginalId),
                x => x.VideoCount);

            // 4) Total video theo Unit (ưu tiên Result, fallback LastVersion)
            var totalByUnit = moduleLessons
                .Select(x => new
                {
                    x.UnitId,
                    VideoCount = resultDict.TryGetValue((x.UnitId, x.LessonOriginalId), out var v)
                        ? v
                        : x.VideoCount
                })
                .GroupBy(x => x.UnitId)
                .Select(g => new
                {
                    UnitId = g.Key,
                    TotalCount = g.Sum(x => x.VideoCount)
                })
                .ToList();

            // 5) Percent theo Unit
            var doneDict = doneByUnit.ToDictionary(x => x.UnitId, x => x.DoneCount);

            return totalByUnit
                .Select(x =>
                {
                    var done = doneDict.TryGetValue(x.UnitId, out var d) ? d : 0;
                    var percent = NumberHelper.GetPercent(done, x.TotalCount);
                    return (x.UnitId, percent);
                })
                .ToList();
        }

        public async Task<List<(Guid UnitId, double Percent)>> GetUnitVideoUnitTestPercentsAsync(Guid courseResultId,
        IReadOnlyCollection<Guid> unitIds,
        CancellationToken cancellationToken)
        {
            // 1) Done video theo Unit
            var doneByUnit = await
            (
                from lr in _lessonResultRepository.ReadQueryable
                join vr in _videoResultRepository.ReadQueryable on lr.Id equals vr.LessonResultId
                join vtcr in _videoTimeCodeResultRepository.ReadQueryable on vr.Id equals vtcr.VideoResultId
                join vtc in _videoTimeCodeRepository.ReadQueryable on vtcr.VideoTimeCodeId equals vtc.Id
                where lr.CourseResultId == courseResultId && vtcr.Status == EnumResultStatus.Done
                && vtc.TimeCodeType == EnumTimeCodeType.UnitTest
                group vtcr.Id by lr.UnitId into g
                select new
                {
                    UnitId = g.Key,
                    DoneCount = g.Select(x => x).Distinct().Count()
                }
            ).ToListAsync(cancellationToken);

            // 2) Lessons cố định của Unit (LastVersion) theo OriginalId
            var moduleLessons = await
            (
                from um in _unitModuleRepository.ReadQueryable
                join l in _lessonRepository.ReadQueryable on um.OriginalId equals l.OriginalId
                join lm in _lessonModuleRepository.ReadQueryable on l.Id equals lm.LessonId
                join v in _videoRepository.ReadQueryable on lm.OriginalId equals v.OriginalId
                where unitIds.Contains(um.UnitId) // ✅ đúng key
                      && um.UnitConfigType == EnumUnitConfigType.Lesson
                      && l.VersionStatus == EnumVersionStatus.LastVersion
                      && v.VersionStatus == EnumVersionStatus.LastVersion
                select new
                {
                    UnitId = um.UnitId,            // ✅ đúng UnitId
                    VideoOriginalId = v.OriginalId,
                    VideoTimeCodeCount = _videoTimeCodeRepository.ReadQueryable
                                                                 .Where(x => x.TimeCodeType == EnumTimeCodeType.UnitTest)
                                                                 .Where(x => x.VideoId == v.Id)
                                                                 .Count()
                }
            ).ToListAsync(cancellationToken);

            // 3) Lessons đã có Result (ưu tiên đúng version)
            var resultLessonMap = await
            (
                from lr in _lessonResultRepository.ReadQueryable
                join vr in _videoResultRepository.ReadQueryable on lr.Id equals vr.LessonResultId
                join v in _videoRepository.ReadQueryable on vr.VideoId equals v.Id
                where lr.CourseResultId == courseResultId
                select new
                {
                    UnitId = lr.UnitId,
                    VideoOriginalId = v.OriginalId,
                    VideoTimeCodeCount = (from baseQ in _videoTimeCodeResultRepository.ReadQueryable
                                          join vtc in _videoTimeCodeRepository.ReadQueryable on baseQ.VideoTimeCodeId equals vtc.Id
                                          where vtc.TimeCodeType == EnumTimeCodeType.UnitTest && baseQ.VideoResultId == vr.Id
                                          && baseQ.Status == EnumResultStatus.Done
                                          select baseQ.Id).Count(),
                }
            ).ToListAsync(cancellationToken);

            var resultDict = resultLessonMap.ToDictionary(
                x => (x.UnitId, x.VideoOriginalId),
                x => x.VideoTimeCodeCount);

            // 4) Total video theo Unit (ưu tiên Result, fallback LastVersion)
            var totalByUnit = moduleLessons
                .Select(x => new
                {
                    x.UnitId,
                    VideoTimeCodeCount = resultDict.TryGetValue((x.UnitId, x.VideoOriginalId), out var v)
                        ? v
                        : x.VideoTimeCodeCount
                })
                .GroupBy(x => x.UnitId)
                .Select(g => new
                {
                    UnitId = g.Key,
                    TotalCount = g.Sum(x => x.VideoTimeCodeCount)
                })
                .ToList();

            // 5) Percent theo Unit
            var doneDict = doneByUnit.ToDictionary(x => x.UnitId, x => x.DoneCount);

            return totalByUnit
                .Select(x =>
                {
                    var done = doneDict.TryGetValue(x.UnitId, out var d) ? d : 0;
                    var percent = NumberHelper.GetPercent(done, x.TotalCount);
                    return (x.UnitId, percent);
                })
                .ToList();
        }

        public async Task<List<(Guid UnitId, double Percent)>> GetUnitHomeWorkPercentsAsync(Guid courseResultId,
        IReadOnlyCollection<Guid> unitIds,
        CancellationToken cancellationToken)
        {
            // 1) Done video theo Unit
            var doneByUnit = await
            (
                from lr in _lessonResultRepository.ReadQueryable
                join hr in _homeWorkResultRepository.ReadQueryable
                    on lr.Id equals hr.LessonResultId
                where lr.CourseResultId == courseResultId
                      && hr.Status == EnumResultStatus.Done
                group hr.Id by lr.UnitId into g
                select new
                {
                    UnitId = g.Key,
                    DoneCount = g.Select(x => x).Distinct().Count()
                }
            ).ToListAsync(cancellationToken);

            // 2) Lessons cố định của Unit (LastVersion) theo OriginalId
            var moduleLessons = await
            (
                from um in _unitModuleRepository.ReadQueryable
                join l in _lessonRepository.ReadQueryable on um.OriginalId equals l.OriginalId
                where unitIds.Contains(um.UnitId) // ✅ đúng key
                      && um.UnitConfigType == EnumUnitConfigType.Lesson
                      && l.VersionStatus == EnumVersionStatus.LastVersion
                select new
                {
                    UnitId = um.UnitId,            // ✅ đúng UnitId
                    LessonOriginalId = l.OriginalId,
                    HomeWorkCount = l.HomeWorkCount
                }
            ).ToListAsync(cancellationToken);

            // 3) Lessons đã có Result (ưu tiên đúng version)
            var resultLessonMap = await
            (
                from lr in _lessonResultRepository.ReadQueryable
                join l in _lessonRepository.ReadQueryable on lr.LessonId equals l.Id
                where lr.CourseResultId == courseResultId
                select new
                {
                    UnitId = lr.UnitId,
                    LessonOriginalId = l.OriginalId,
                    HomeWorkCount = l.HomeWorkCount
                }
            ).ToListAsync(cancellationToken);

            var resultDict = resultLessonMap.ToDictionary(
                x => (x.UnitId, x.LessonOriginalId),
                x => x.HomeWorkCount);

            // 4) Total video theo Unit (ưu tiên Result, fallback LastVersion)
            var totalByUnit = moduleLessons
                .Select(x => new
                {
                    x.UnitId,
                    HomeWorkCount = resultDict.TryGetValue((x.UnitId, x.LessonOriginalId), out var v)
                        ? v
                        : x.HomeWorkCount
                })
                .GroupBy(x => x.UnitId)
                .Select(g => new
                {
                    UnitId = g.Key,
                    TotalCount = g.Sum(x => x.HomeWorkCount)
                })
                .ToList();

            // 5) Percent theo Unit
            var doneDict = doneByUnit.ToDictionary(x => x.UnitId, x => x.DoneCount);

            return totalByUnit
                .Select(x =>
                {
                    var done = doneDict.TryGetValue(x.UnitId, out var d) ? d : 0;
                    var percent = NumberHelper.GetPercent(done, x.TotalCount);
                    return (x.UnitId, percent);
                })
                .ToList();
        }

        public async Task<List<(Guid UnitId, double Percent)>> GetUnitClassForumPercentsAsync(Guid courseResultId,
        IReadOnlyCollection<Guid> unitIds,
        CancellationToken cancellationToken)
        {
            // 1) Done video theo Unit
            var doneByUnit = await
            (
                from lr in _lessonResultRepository.ReadQueryable
                join clr in _classForumResultRepository.ReadQueryable
                    on lr.Id equals clr.LessonResultId
                where lr.CourseResultId == courseResultId
                      && clr.ResultStatus == EnumResultStatus.Done
                group clr.Id by lr.UnitId into g
                select new
                {
                    UnitId = g.Key,
                    DoneCount = g.Select(x => x).Distinct().Count()
                }
            ).ToListAsync(cancellationToken);

            // 2) Lessons cố định của Unit (LastVersion) theo OriginalId
            var moduleLessons = await
            (
                from um in _unitModuleRepository.ReadQueryable
                join l in _lessonRepository.ReadQueryable on um.OriginalId equals l.OriginalId
                where unitIds.Contains(um.UnitId) // ✅ đúng key
                      && um.UnitConfigType == EnumUnitConfigType.Lesson
                      && l.VersionStatus == EnumVersionStatus.LastVersion
                select new
                {
                    UnitId = um.UnitId,            // ✅ đúng UnitId
                    LessonOriginalId = l.OriginalId,
                    HomeWorkCount = l.HomeWorkCount
                }
            ).ToListAsync(cancellationToken);

            // 3) Lessons đã có Result (ưu tiên đúng version)
            var resultLessonMap = await
            (
                from lr in _lessonResultRepository.ReadQueryable
                join l in _lessonRepository.ReadQueryable on lr.LessonId equals l.Id
                where lr.CourseResultId == courseResultId
                select new
                {
                    UnitId = lr.UnitId,
                    LessonOriginalId = l.OriginalId,
                    ClassForumCount = l.ClassForumCount
                }
            ).ToListAsync(cancellationToken);

            var resultDict = resultLessonMap.ToDictionary(
                x => (x.UnitId, x.LessonOriginalId),
                x => x.ClassForumCount);

            // 4) Total video theo Unit (ưu tiên Result, fallback LastVersion)
            var totalByUnit = moduleLessons
                .Select(x => new
                {
                    x.UnitId,
                    ClassForumCount = resultDict.TryGetValue((x.UnitId, x.LessonOriginalId), out var v)
                        ? v
                        : x.HomeWorkCount
                })
                .GroupBy(x => x.UnitId)
                .Select(g => new
                {
                    UnitId = g.Key,
                    TotalCount = g.Sum(x => x.ClassForumCount)
                })
                .ToList();

            // 5) Percent theo Unit
            var doneDict = doneByUnit.ToDictionary(x => x.UnitId, x => x.DoneCount);

            return totalByUnit
                .Select(x =>
                {
                    var done = doneDict.TryGetValue(x.UnitId, out var d) ? d : 0;
                    var percent = NumberHelper.GetPercent(done, x.TotalCount);
                    return (x.UnitId, percent);
                })
                .ToList();
        }
    }
}
