// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.Enums;
    using Fsel.Course.Domain.Entities.V1i1;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels.CachingModels;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.CacheServices.BuildModules;
    using Microsoft.EntityFrameworkCore;

    public interface ICourseService
    {
        Task<CourseBuildModel> GetCourseBuildModel(Guid courseId);
    }

    public sealed class CourseService : ICourseService
    {
        private readonly ICourseBuildCachingService _courseBuildCachingService;
        private readonly ICourseRepository _courseRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly ITestRepository _testRepository;
        private readonly IHomeWorkRepository _homeWorkRepository;
        private readonly IClassForumRepository _classForumRepository;
        private readonly IVideoRepository _videoRepository;
        private readonly IDocumentRepository _documentRepository;

        public CourseService(ICourseBuildCachingService courseBuildCachingService,
            ICourseRepository courseRepository,
            IUnitRepository unitRepository,
            ILessonRepository lessonRepository,
            ITestRepository testRepository,
            IHomeWorkRepository homeWorkRepository,
            IClassForumRepository classForumRepository,
            IVideoRepository videoRepository,
            IDocumentRepository documentRepository)
        {
            _courseBuildCachingService = courseBuildCachingService;
            _courseRepository = courseRepository;
            _unitRepository = unitRepository;
            _lessonRepository = lessonRepository;
            _testRepository = testRepository;
            _homeWorkRepository = homeWorkRepository;
            _classForumRepository = classForumRepository;
            _videoRepository = videoRepository;
            _documentRepository = documentRepository;
        }

        public async Task<CourseBuildModel> GetCourseBuildModel(Guid courseId)
        {
            return await _courseBuildCachingService.GetOrSetAsync(
                courseId.ToString(),
                async (_, ct) =>
                {
                    var course = await LoadCourseAsync(courseId, ct);
                    if (course == null)
                    {
                        return new CourseBuildModel { CourseId = courseId };
                    }

                    var ctx = await LoadBuildContextAsync(course, ct);
                    return BuildCourseModel(courseId, course, ctx);
                });
        }

        // =========================
        // 1) LOAD ROOT (COURSE)
        // =========================
        private async Task<Domain.Entities.Course?> LoadCourseAsync(Guid courseId, CancellationToken ct)
        {
            return await _courseRepository.ReadQueryable
                .AsNoTracking()
                .Include(x => x.CourseModules)
                .FirstOrDefaultAsync(x => x.Id == courseId, ct);
        }

        // =========================
        // 2) LOAD CONTEXT (ALL DATA)
        // =========================
        private async Task<CourseBuildContext> LoadBuildContextAsync(Domain.Entities.Course course, CancellationToken ct)
        {
            var unitOriginalIds = course.CourseModules
                .Where(x => x.CourseConfigType == EnumCourseConfigType.Unit)
                .Select(x => x.OriginalId)
                .Distinct()
                .ToList();

            var courseTestOriginalIds = course.CourseModules
                .Where(x => x.CourseConfigType == EnumCourseConfigType.Test)
                .Select(x => x.OriginalId)
                .Distinct()
                .ToList();

            var units = await LoadUnitsAsync(unitOriginalIds, ct);

            var lessonOriginalIds = units.SelectMany(x => x.UnitModules)
                .Where(x => x.UnitConfigType == EnumUnitConfigType.Lesson)
                .Select(x => x.OriginalId)
                .Distinct()
                .ToList();

            var unitTestOriginalIds = units.SelectMany(x => x.UnitModules)
                .Where(x => x.UnitConfigType == EnumUnitConfigType.Test)
                .Select(x => x.OriginalId)
                .Distinct()
                .ToList();

            var lessons = await LoadLessonsAsync(lessonOriginalIds, ct);

            var homeworkOriginalIds = lessons.SelectMany(x => x.LessonModules)
                .Where(x => x.LessonConfigType == EnumLessonConfigType.HomeWork)
                .Select(x => x.OriginalId)
                .Distinct()
                .ToList();

            var videoOriginalIds = lessons.SelectMany(x => x.LessonModules)
                .Where(x => x.LessonConfigType == EnumLessonConfigType.Video)
                .Select(x => x.OriginalId)
                .Distinct()
                .ToList();

            var classForumOriginalIds = lessons.SelectMany(x => x.LessonModules)
                .Where(x => x.LessonConfigType == EnumLessonConfigType.ClassForum)
                .Select(x => x.OriginalId)
                .Distinct()
                .ToList();

            var documentOriginalIds = lessons.SelectMany(x => x.LessonModules)
                .Where(x => x.LessonConfigType == EnumLessonConfigType.Document)
                .Select(x => x.OriginalId)
                .Distinct()
                .ToList();

            var homeWorks = await LoadHomeWorksAsync(homeworkOriginalIds, ct);
            var videos = await LoadVideosAsync(videoOriginalIds, ct);
            var classForums = await LoadClassForumsAsync(classForumOriginalIds, ct);
            var documents = await LoadDocumentsAsync(documentOriginalIds, ct);

            var allTestOriginalIds = courseTestOriginalIds
                .Concat(unitTestOriginalIds)
                .Distinct()
                .ToList();

            var tests = await LoadTestsAsync(allTestOriginalIds, ct);

            return CourseBuildContext.Create(units, lessons, tests, homeWorks, videos, classForums, documents);
        }

        private async Task<List<Domain.Entities.Unit>> LoadUnitsAsync(List<Guid> unitOriginalIds, CancellationToken ct)
        {
            if (unitOriginalIds.Count == 0)
            {
                return new List<Domain.Entities.Unit>();
            }
            return await _unitRepository.ReadQueryable
                .AsNoTracking()
                .Include(x => x.UnitModules)
                .Where(x => unitOriginalIds.Contains(x.OriginalId))
                .Where(x => x.VersionStatus == EnumVersionStatus.LastVersion)
                .ToListAsync(ct);
        }

        private async Task<List<Domain.Entities.Lesson>> LoadLessonsAsync(List<Guid> lessonOriginalIds, CancellationToken ct)
        {
            if (lessonOriginalIds.Count == 0)
            {
                return new List<Domain.Entities.Lesson>();
            }
            return await _lessonRepository.ReadQueryable
                .AsNoTracking()
                .Include(x => x.LessonModules)
                .Where(x => lessonOriginalIds.Contains(x.OriginalId))
                .Where(x => x.VersionStatus == EnumVersionStatus.LastVersion)
                .ToListAsync(ct);
        }

        private async Task<List<Domain.Entities.HomeWork>> LoadHomeWorksAsync(List<Guid> originalIds, CancellationToken ct)
        {
            if (originalIds.Count == 0)
            {
                return new List<Domain.Entities.HomeWork>();
            }
            return await _homeWorkRepository.ReadQueryable
                .AsNoTracking()
                .Where(x => originalIds.Contains(x.OriginalId))
                .Where(x => x.VersionStatus == EnumVersionStatus.LastVersion)
                .ToListAsync(ct);
        }

        private async Task<List<Domain.Entities.Video>> LoadVideosAsync(List<Guid> originalIds, CancellationToken ct)
        {
            if (originalIds.Count == 0)
            {
                return new List<Domain.Entities.Video>();
            }
            return await _videoRepository.ReadQueryable
                .AsNoTracking()
                .Where(x => originalIds.Contains(x.OriginalId))
                .Where(x => x.VersionStatus == EnumVersionStatus.LastVersion)
                .ToListAsync(ct);
        }

        private async Task<List<Domain.Entities.ClassForum>> LoadClassForumsAsync(List<Guid> originalIds, CancellationToken ct)
        {
            if (originalIds.Count == 0)
            {
                return new List<Domain.Entities.ClassForum>();
            }
            return await _classForumRepository.ReadQueryable
                .AsNoTracking()
                .Where(x => originalIds.Contains(x.OriginalId))
                .Where(x => x.VersionStatus == EnumVersionStatus.LastVersion)
                .ToListAsync(ct);
        }

        private async Task<List<Domain.Entities.Document>> LoadDocumentsAsync(List<Guid> originalIds, CancellationToken ct)
        {
            if (originalIds.Count == 0)
            {
                return new List<Domain.Entities.Document>();
            }
            return await _documentRepository.ReadQueryable
                .AsNoTracking()
                .Where(x => originalIds.Contains(x.OriginalId))
                .Where(x => x.VersionStatus == EnumVersionStatus.LastVersion)
                .ToListAsync(ct);
        }

        private async Task<List<Domain.Entities.TestConfigs.Test>> LoadTestsAsync(List<Guid> originalIds, CancellationToken ct)
        {
            if (originalIds.Count == 0)
            {
                return new List<Domain.Entities.TestConfigs.Test>();
            }
            return await _testRepository.ReadQueryable
                .AsNoTracking()
                .Where(x => originalIds.Contains(x.OriginalId))
                .Where(x => x.VersionStatus == EnumVersionStatus.LastVersion)
                .ToListAsync(ct);
        }

        // =========================
        // 3) BUILD MODEL
        // =========================
        private static CourseBuildModel BuildCourseModel(
            Guid courseId,
            Domain.Entities.Course course,
            CourseBuildContext ctx)
        {
            var model = new CourseBuildModel
            {
                CourseId = courseId,
                CourseModules = course.CourseModules
                    .OrderBy(x => x.DisplayOrder)
                    .Select(cm => BuildCourseModule(cm, ctx))
                    .ToList()
            };

            return model;
        }

        private static CourseModuleBuildModel BuildCourseModule(
            CourseModule cm,
            CourseBuildContext ctx)
        {
            var cmBuild = new CourseModuleBuildModel
            {
                Id = cm.Id,
                ConfigType = cm.CourseConfigType,
                OriginalId = cm.OriginalId,
                DisplayOrder = cm.DisplayOrder,
                OpenOrder = cm.OpenOrder,
                DisplayNumber = cm.DisplayNumber,
                Percent = cm.Percent
            };

            switch (cm.CourseConfigType)
            {
                case EnumCourseConfigType.Unit:
                    FillUnitCourseModule(cmBuild, cm.OriginalId, ctx);
                    break;

                case EnumCourseConfigType.Test:
                    cmBuild.TestId = ctx.TryGetTestId(cm.OriginalId);
                    break;
            }

            return cmBuild;
        }

        private static void FillUnitCourseModule(
            CourseModuleBuildModel cmBuild,
            Guid unitOriginalId,
            CourseBuildContext ctx)
        {
            if (!ctx.TryGetUnit(unitOriginalId, out var unit))
            {
                return;
            }

            cmBuild.UnitId = unit.Id;

            cmBuild.UnitModuleBuilds = unit.UnitModules
                .OrderBy(x => x.DisplayOrder)
                .Select(um => BuildUnitModule(um, ctx))
                .ToList();
        }

        private static UnitModuleBuildModel BuildUnitModule(
            UnitModule um,
            CourseBuildContext ctx)
        {
            var umBuild = new UnitModuleBuildModel
            {
                Id = um.Id,
                ConfigType = um.UnitConfigType,
                OriginalId = um.OriginalId,
                DisplayOrder = um.DisplayOrder,
                OpenOrder = um.OpenOrder,
                DisplayNumber = um.DisplayNumber,
                Percent = um.Percent
            };

            switch (um.UnitConfigType)
            {
                case EnumUnitConfigType.Lesson:
                    FillLessonUnitModule(umBuild, um.OriginalId, ctx);
                    break;

                case EnumUnitConfigType.Test:
                    umBuild.TestId = ctx.TryGetTestId(um.OriginalId);
                    break;
            }

            return umBuild;
        }

        private static void FillLessonUnitModule(
            UnitModuleBuildModel umBuild,
            Guid lessonOriginalId,
            CourseBuildContext ctx)
        {
            if (!ctx.TryGetLesson(lessonOriginalId, out var lesson))
            {
                return;
            }

            umBuild.LessonId = lesson.Id;
            umBuild.LessonModuleBuilds = lesson.LessonModules
                .OrderBy(x => x.DisplayOrder)
                .Select(lm => BuildLessonModule(lm, ctx))
                .ToList();
        }

        private static LessonModuleBuildModel BuildLessonModule(
            LessonModule lm,
            CourseBuildContext ctx)
        {
            var lmBuild = new LessonModuleBuildModel
            {
                Id = lm.Id,
                ConfigType = lm.LessonConfigType,
                OriginalId = lm.OriginalId,
                DisplayOrder = lm.DisplayOrder,
                OpenOrder = lm.OpenOrder,
                DisplayNumber = lm.DisplayNumber,
                Percent = lm.Percent
            };

            switch (lm.LessonConfigType)
            {
                case EnumLessonConfigType.HomeWork:
                    lmBuild.HọmeWorkId = ctx.TryGetHomeWorkId(lm.OriginalId);
                    break;

                case EnumLessonConfigType.Video:
                    lmBuild.VideoId = ctx.TryGetVideoId(lm.OriginalId);
                    break;

                case EnumLessonConfigType.ClassForum:
                    lmBuild.ClassForumId = ctx.TryGetClassForumId(lm.OriginalId);
                    break;

                case EnumLessonConfigType.Document:
                    lmBuild.DocumentId = ctx.TryGetDocumentId(lm.OriginalId);
                    break;
            }

            return lmBuild;
        }

        // =========================
        // 4) CONTEXT (LOOKUP/DICTS)
        // =========================
        private sealed class CourseBuildContext
        {
            private readonly Dictionary<Guid, Domain.Entities.Unit> _unitByOriginalId;
            private readonly Dictionary<Guid, Domain.Entities.Lesson> _lessonByOriginalId;
            private readonly Dictionary<Guid, Guid> _testIdByOriginalId;

            private readonly Dictionary<Guid, Guid> _homeWorkIdByOriginalId;
            private readonly Dictionary<Guid, Guid> _classForumIdByOriginalId;
            private readonly Dictionary<Guid, Guid> _documentIdByOriginalId;
            private readonly Dictionary<Guid, Guid> _videoIdByOriginalId;

            private CourseBuildContext(
                Dictionary<Guid, Domain.Entities.Unit> unitByOriginalId,
                Dictionary<Guid, Domain.Entities.Lesson> lessonByOriginalId,
                Dictionary<Guid, Guid> testIdByOriginalId,
                Dictionary<Guid, Guid> homeWorkIdByOriginalId,
                Dictionary<Guid, Guid> videoIdByOriginalId,
                Dictionary<Guid, Guid> classForumIdByOriginalId,
                Dictionary<Guid, Guid> documentIdByOriginalId)
            {
                _unitByOriginalId = unitByOriginalId;
                _lessonByOriginalId = lessonByOriginalId;
                _testIdByOriginalId = testIdByOriginalId;
                _homeWorkIdByOriginalId = homeWorkIdByOriginalId;
                _videoIdByOriginalId = videoIdByOriginalId;
                _classForumIdByOriginalId = classForumIdByOriginalId;
                _documentIdByOriginalId = documentIdByOriginalId;
            }

            public static CourseBuildContext Create(
                List<Domain.Entities.Unit> units,
                List<Domain.Entities.Lesson> lessons,
                List<Domain.Entities.TestConfigs.Test> tests,
                List<Domain.Entities.HomeWork> homeWorks,
                List<Domain.Entities.Video> videos,
                List<Domain.Entities.ClassForum> classForums,
                List<Domain.Entities.Document> documents)
            {
                var unitByOriginalId = units
                    .GroupBy(x => x.OriginalId)
                    .ToDictionary(g => g.Key, g => g.First());

                var lessonByOriginalId = lessons
                    .GroupBy(x => x.OriginalId)
                    .ToDictionary(g => g.Key, g => g.First());

                var testIdByOriginalId = tests
                    .GroupBy(x => x.OriginalId)
                    .ToDictionary(g => g.Key, g => g.First().Id);

                var homeWorkIdByOriginalId = homeWorks
                    .GroupBy(x => x.OriginalId)
                    .ToDictionary(g => g.Key, g => g.First().Id);

                var classForumIdByOriginalId = classForums
                    .GroupBy(x => x.OriginalId)
                    .ToDictionary(g => g.Key, g => g.First().Id);

                var documentIdByOriginalId = documents
                    .GroupBy(x => x.OriginalId)
                    .ToDictionary(g => g.Key, g => g.First().Id);

                var videoIdByOriginalId = videos
                    .GroupBy(x => x.OriginalId)
                    .ToDictionary(g => g.Key, g => g.First().Id);

                return new CourseBuildContext(
                    unitByOriginalId,
                    lessonByOriginalId,
                    testIdByOriginalId,
                    homeWorkIdByOriginalId,
                    videoIdByOriginalId,
                    classForumIdByOriginalId,
                    documentIdByOriginalId);
            }

            public bool TryGetUnit(Guid originalId, out Domain.Entities.Unit unit)
                => _unitByOriginalId.TryGetValue(originalId, out unit!);

            public bool TryGetLesson(Guid originalId, out Domain.Entities.Lesson lesson)
                => _lessonByOriginalId.TryGetValue(originalId, out lesson!);

            public Guid? TryGetTestId(Guid originalId)
                => _testIdByOriginalId.TryGetValue(originalId, out var id) ? id : (Guid?)null;

            public Guid? TryGetHomeWorkId(Guid originalId)
                => _homeWorkIdByOriginalId.TryGetValue(originalId, out var id) ? id : (Guid?)null;

            public Guid? TryGetVideoId(Guid originalId)
                => _videoIdByOriginalId.TryGetValue(originalId, out var id) ? id : (Guid?)null;

            public Guid? TryGetClassForumId(Guid originalId)
                => _classForumIdByOriginalId.TryGetValue(originalId, out var id) ? id : (Guid?)null;

            public Guid? TryGetDocumentId(Guid originalId)
                => _documentIdByOriginalId.TryGetValue(originalId, out var id) ? id : (Guid?)null;
        }
    }
}
