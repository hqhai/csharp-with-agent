// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.Enums;
using Fsel.Core.Base;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Entities.V1i1;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.IRepositories;
using Fsel.Shared.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Infrastructure.Repositories
{
    public class LessonRepository : BaseRepository<Lesson>, ILessonRepository
    {
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IClassForumRepository _classForumRepository;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly IUnitModuleRepository _unitModuleRepository;

        public LessonRepository(CourseDbContext dbContext,
            CourseReadDbContext readDbContext,
            ILessonResultRepository lessonResultRepository,
            IClassForumRepository classForumRepository,
            IVideoResultRepository videoResultRepository,
            IUnitModuleRepository unitModuleRepository,

            AuthContext authContext,
            AutoMapper.IMapper mapper) : base(dbContext, readDbContext, authContext, mapper)
        {
            _lessonResultRepository = lessonResultRepository;
            _classForumRepository = classForumRepository;
            _videoResultRepository = videoResultRepository;
            _unitModuleRepository = unitModuleRepository;
        }

        public async Task<Lesson?> GetIncludeByIdNoTrackingAsync(Guid id)
        {
            try
            {
                return await Queryable.Include(e => e.UnitLessons.Where(n => !n.IsDeleted))
                                 .Include(e => e.ClassForum)
                                 .ThenInclude(e => e!.ClassForumFiles.OrderBy(x => x.CreatedDate))
                                 .Include(e => e.LessonExtraPractices.Where(n => !n.IsDeleted).OrderBy(x => x.CreatedDate))
                                 .Include(e => e.LessonInstructions.Where(n => !n.IsDeleted).OrderBy(x => x.CreatedDate))
                                 .ThenInclude(x => x.Skill)
                                 .Include(e => e.LessonVideos.Where(n => !n.IsDeleted).OrderBy(x => x.CreatedDate))
                                 .ThenInclude(e => e.Video)
                                 .ThenInclude(e => e!.VideoTimeCodes.OrderBy(x => x.CreatedDate))
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<double> GetPercentHomeWork(Guid courseId, Guid unitId, Guid? studentId)
        {
            var lessonResults = await _lessonResultRepository.ReadQueryable.Include(x => x.Lesson)
                                                             .Include(x => x.VideoResults)
                                                             .Where(x => x.CourseId == courseId && x.UnitId == unitId && x.StudentId == studentId)
                                                             .ToListAsync();

            var lessonResultIds = lessonResults.Select(x => x.Id).ToList();
            var lessonIds = lessonResults.Select(x => x.Lesson!.Id).ToList();

            var lessons = await Queryable.Include(x => x.LessonResults.Where(x => lessonResultIds.Contains(x.Id)))
                                         .ThenInclude(x => x.HomeWorkResults.Where(x => lessonResultIds.Contains(x.LessonResultId)))
                                         .Include(x => x.LessonHomeWorks)
                                         .Where(x => lessonIds.Contains(x.Id)).ToListAsync();

            var listDones = lessons.Select(x => new
            {
                CountDone = x.LessonResults.Where(x => lessonResultIds.Contains(x.Id)).SelectMany(x => x.HomeWorkResults).Where(x => x.Status == EnumResultStatus.Done).Count(),
                TotalDone = x.LessonHomeWorks.Count
            }).ToList();
            return NumberHelper.GetPercent(listDones.Sum(x => x.CountDone), listDones.Sum(x => x.TotalDone));
        }

        public async Task<double> GetPercentClassForum(Guid courseId, Guid unitId, Guid? studentId)
        {
            var lessonResults = await _lessonResultRepository.ReadQueryable.Include(x => x.Lesson)
                                                             .Include(x => x.VideoResults)
                                                             .Where(x => x.CourseId == courseId && x.UnitId == unitId && x.StudentId == studentId)
                                                             .ToListAsync();
            var lessonResultIds = lessonResults.Select(x => x.Id).ToList();
            var lessonIds = lessonResults.Select(x => x.Lesson!.Id).ToList();

            var classForums = await _classForumRepository.ReadQueryable.Include(x => x.ClassForumResults.Where(x => lessonResultIds.Contains(x.LessonResultId)))
                                                         .Where(x => x.LessonId.HasValue && lessonIds.Contains(x.LessonId.Value))
                                                         .ToListAsync();

            var listDones = classForums.Select(x => new
            {
                CountDone = x.ClassForumResults.Where(x => lessonResultIds.Contains(x.LessonResultId)).Where(x => x.Status.HasValue).Count(),
                TotalDone = 1
            }).ToList();
            return NumberHelper.GetPercent(listDones.Sum(x => x.CountDone), listDones.Sum(x => x.TotalDone));
        }

        public async Task<double> GetPercentLesson(Guid courseId, Guid unitId, Guid? studentId)
        {
            var lessonResults = await _lessonResultRepository.ReadQueryable.Where(x => x.CourseId == courseId && x.UnitId == unitId && x.StudentId == studentId).ToListAsync();
            var listDones = lessonResults.Select(x => new
            {
                CountDone = x.Status == EnumResultStatus.Done ? 1 : default,
                TotalDone = 1
            }).ToList();
            return NumberHelper.GetPercent(listDones.Sum(x => x.CountDone), listDones.Sum(x => x.TotalDone));
        }

        public override async Task<Lesson?> GetIncludeByIdAsync(Guid id)
        {
            try
            {
                return await Queryable.Include(e => e.UnitLessons.Where(n => !n.IsDeleted).OrderBy(x => x.CreatedDate))
                                 .Include(e => e.ClassForum)
                                 .ThenInclude(e => e!.ClassForumFiles.OrderBy(x => x.CreatedDate))
                                 .Include(x => x.LessonResults.Where(n => !n.IsDeleted).OrderBy(x => x.CreatedDate))
                                 .Include(e => e.LessonHomeWorks.Where(n => !n.IsDeleted).OrderBy(x => x.CreatedDate))
                                 .ThenInclude(e => e.HomeWork)
                                 .Include(e => e.LessonExtraPractices.Where(n => !n.IsDeleted).OrderBy(x => x.CreatedDate))
                                 .Include(e => e.LessonInstructions.Where(n => !n.IsDeleted).OrderBy(x => x.CreatedDate))
                                 .Include(e => e.LessonVideos.Where(n => !n.IsDeleted).OrderBy(x => x.CreatedDate))
                                 .ThenInclude(x => x.Video)
                                 .ThenInclude(x => x!.VideoTimeCodes.Where(n => !n.IsDeleted).OrderBy(x => x.CreatedDate))
                                 .FirstOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Lesson?> GetIncludeVideoByIdAsync(Guid id)
        {
            try
            {
                return await Queryable.Include(x => x.LessonResults.Where(n => !n.IsDeleted))
                                      .Include(x => x.LessonVideos.Where(n => !n.IsDeleted))
                                      .ThenInclude(x => x.Video)
                                      .FirstOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> IsLessonUsed(Guid id)
        {
            return await Queryable
                 .Include(x => x.UnitLessons.Where(n => !n.IsDeleted))
                 .AnyAsync(x => x.Id == id && x.UnitLessons.Count > 0);
        }

        public async Task<Lesson?> GetAsync(Guid? lessonId)
        {
            return await Queryable.Include(x => x.LessonInstructions).FirstOrDefaultAsync(x => x.Id == lessonId);
        }

        public async Task<IDictionary<Guid, Lesson>> GetLessonDicAsync(IList<Guid>? originalIds)
        {
            if (originalIds == null || originalIds.Count == 0)
            {
                return new Dictionary<Guid, Lesson>();
            }

            var lessons = await ReadQueryable.WhereBulkContains(originalIds, x => x.OriginalId)
                                            .Where(x => x.VersionStatus == EnumVersionStatus.LastVersion)
                                            .ToListAsync();

            return lessons.ToDictionary(x => x.OriginalId);
        }

        public async Task<(IDictionary<Guid, (Lesson, LessonResult)>, IDictionary<Guid, Lesson>)>
        BuildLessonLookupsAsync(UnitResult unitResult, IList<UnitModule> unitModules)
        {
            var lessonOriginalIds = unitModules.Where(x => x.UnitConfigType == EnumUnitConfigType.Lesson)
                                               .Select(x => x.OriginalId)
                                               .Distinct()
                                               .ToList();

            if (!lessonOriginalIds.Any())
            {
                return (new Dictionary<Guid, (Lesson, LessonResult)>(),
                        new Dictionary<Guid, Lesson>());
            }

            var lessonResults = await (from baseQ in _lessonResultRepository.ReadQueryable
                                       where baseQ.UnitResultId == unitResult.Id
                                       join lesson in ReadQueryable on baseQ.LessonId equals lesson.Id
                                       select new
                                       {
                                           Lesson = lesson,
                                           LessonResult = baseQ
                                       }).ToListAsync();

            var lessonOriginalIdsHasResult = lessonResults
                .Where(x => x.Lesson != null)
                .Select(x => x.Lesson!.OriginalId)
                .Distinct();

            var pendingLessonOriginalIds = lessonOriginalIds
                .Except(lessonOriginalIdsHasResult)
                .ToList();

            var lessonResultDics = await GetLessonDicAsync(pendingLessonOriginalIds);

            var homeWorkResultsByOriginalId = lessonResults
                .Where(x => x.Lesson != null)
                .ToDictionary(
                    x => x.Lesson!.OriginalId,
                    x => (Lesson: x.Lesson!, LessonResult: x.LessonResult));

            return (homeWorkResultsByOriginalId, lessonResultDics);
        }

        private async Task<List<(Guid UnitId, double Percent)>> GetUnitVideoPercentsAsync(
        Guid courseResultId,
        IReadOnlyCollection<Guid> unitIds,
        CancellationToken cancellationToken)
        {
            // 1) Done video theo Unit
            var doneByUnit = await
            (
                from lr in _lessonResultRepository.ReadQueryable
                join vr in _videoResultRepository.ReadQueryable
                    on lr.Id equals vr.LessonResultId
                where lr.CourseResultId == courseResultId
                      && vr.Status == EnumResultStatus.Done
                group vr by lr.UnitId into g
                select new
                {
                    UnitId = g.Key,
                    DoneCount = g.Select(x => x.Id).Distinct().Count()
                }
            ).ToListAsync(cancellationToken);

            // 2) Lessons cố định của Unit (LastVersion) theo OriginalId
            var moduleLessons = await
            (
                from um in _unitModuleRepository.ReadQueryable
                join l in ReadQueryable
                    on um.OriginalId equals l.OriginalId
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
                join l in ReadQueryable
                    on lr.LessonId equals l.Id
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
    }
}
