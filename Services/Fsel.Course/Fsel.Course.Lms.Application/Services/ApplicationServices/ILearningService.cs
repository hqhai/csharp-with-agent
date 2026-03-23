// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices
{
    using System.Linq;
    using Fsel.Common.Enums;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;

    public class GetLearningTreeFromCourseToTestModel
    {
        public Guid StudentId { get; set; }
        public Guid CourseId { get; set; }
        public Guid? CourseModuleId { get; set; }
    }

    public interface ILearningService
    {
        Task<(int, int)> UnitContentCompleted(Guid studentId, IList<Guid> lessonIds, Guid? courseResultId, CancellationToken cancellationToken);

        Task<(int, int)> LessonContentCompleted(Guid studentId, Guid lessonId, Guid? courseResultId, CancellationToken cancellationToken);

        Task<LearningComponentModel?> GetLearningTreeFromCourseToTest(Guid studentId, Guid courseId, Guid? moduleId = null, CancellationToken cancellationToken = default);

        Task<IList<LearningComponentModel>> GetLearningTreeFromCourseToTest(IList<GetLearningTreeFromCourseToTestModel> studentCourseIds, CancellationToken cancellationToken = default);
    }

    public class LearningService : ILearningService
    {
        private readonly ILessonRepository _lessonRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly IServiceProvider _serviceProvider;
        private readonly IUnitRepository _unitRepository;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly ITestGroupResultRepository _testGroupResultRepository;
        private readonly ITestRepository _testRepository;

        public LearningService(ILessonRepository lessonRepository, ILessonResultRepository lessonResultRepository, ICourseRepository courseRepository, ICourseResultRepository courseResultRepository, IServiceProvider serviceProvider, IUnitRepository unitRepository, IUnitResultRepository unitResultRepository, ITestGroupResultRepository testGroupResultRepository, ITestRepository testRepository)
        {
            _lessonRepository = lessonRepository;
            _lessonResultRepository = lessonResultRepository;
            _courseRepository = courseRepository;
            _courseResultRepository = courseResultRepository;
            _serviceProvider = serviceProvider;
            _unitRepository = unitRepository;
            _unitResultRepository = unitResultRepository;
            _testGroupResultRepository = testGroupResultRepository;
            _testRepository = testRepository;
        }

        public async Task<(int, int)> UnitContentCompleted(Guid studentId, IList<Guid> lessonIds, Guid? courseResultId, CancellationToken cancellationToken)
        {
            var lessons = await _lessonRepository.ReadQueryable.WhereBulkContains(lessonIds, p => p.Id).ToListAsync(cancellationToken);

            var total = lessons.Sum(p => p.VideoCount) + lessons.Sum(p => p.ClassForumCount) + lessons.Sum(p => p.HomeWorkCount) + lessons.Sum(p => p.DocumentCount);

            var lessonResults = await _lessonResultRepository.ReadQueryable.Include(v => v.VideoResults).Include(h => h.HomeWorkResults).Include(c => c.ClassForumResults).Include(d => d.DocumentResults).WhereBulkContains(lessonIds, p => p.LessonId).Where(p => p.StudentId == studentId && p.CourseResultId == courseResultId).ToListAsync(cancellationToken);

            var numberVideoDone = lessonResults.SelectMany(p => p.VideoResults).Where(p => p.Status == EnumResultStatus.Done).Count();
            var numberClassForumDone = lessonResults.SelectMany(p => p.ClassForumResults).Where(p => p.ResultStatus == EnumResultStatus.Done).Count();
            var numberHomeworkDone = lessonResults.SelectMany(p => p.HomeWorkResults).Where(p => p.Status == EnumResultStatus.Done).Count();
            var numberDocumentDone = lessonResults.SelectMany(p => p.DocumentResults).Where(p => p.Status == EnumResultStatus.Done).Count();

            var numberComplete = numberVideoDone + numberClassForumDone + numberHomeworkDone + numberDocumentDone;

            return (numberComplete, total);
        }

        public async Task<(int, int)> LessonContentCompleted(Guid studentId, Guid lessonId, Guid? courseResultId, CancellationToken cancellationToken)
        {
            var lessons = await _lessonRepository.ReadQueryable.FirstOrDefaultAsync(p => p.Id == lessonId, cancellationToken);
            if (lessons == null)
            {
                return (0, 0);
            }

            var total = lessons.VideoCount + lessons.ClassForumCount + lessons.HomeWorkCount + lessons.DocumentCount;

            var lessonResult = await _lessonResultRepository.ReadQueryable.Include(v => v.VideoResults).Include(h => h.HomeWorkResults).Include(c => c.ClassForumResults).Include(d => d.DocumentResults).FirstOrDefaultAsync(p => p.StudentId == studentId && p.LessonId == lessonId && p.CourseResultId == courseResultId, cancellationToken);

            var numberVideoDone = lessonResult?.VideoResults.Where(p => p.Status == EnumResultStatus.Done).Count();
            var numberClassForumDone = lessonResult?.ClassForumResults.Where(p => p.ResultStatus == EnumResultStatus.Done).Count();
            var numberHomeworkDone = lessonResult?.HomeWorkResults.Where(p => p.Status == EnumResultStatus.Done).Count();
            var numberDocumentDone = lessonResult?.DocumentResults.Where(p => p.Status == EnumResultStatus.Done).Count();

            var numberComplete = numberVideoDone + numberClassForumDone + numberHomeworkDone + numberDocumentDone;

            return (numberComplete ?? 0, total);
        }

        public async Task<LearningComponentModel?> GetLearningTreeFromCourseToTest(Guid studentId, Guid courseId, Guid? moduleId = null, CancellationToken cancellationToken = default)
        {
            return (await GetLearningTreeFromCourseToTest(new List<GetLearningTreeFromCourseToTestModel>
            {
                new GetLearningTreeFromCourseToTestModel
                {
                    CourseModuleId = moduleId,
                    StudentId = studentId,
                    CourseId = courseId,
                }
            }, cancellationToken)).FirstOrDefault();
        }

        public async Task<IList<LearningComponentModel>> GetLearningTreeFromCourseToTest(IList<GetLearningTreeFromCourseToTestModel> studentCourseIds, CancellationToken cancellationToken = default)
        {
            var courses = await _courseRepository.ReadQueryable.Include(u => u.CourseModules).Where(cr => studentCourseIds.Select(n => n.CourseId).Contains(cr.Id)).ToListAsync(cancellationToken);
            var courseResults = await _courseResultRepository.ReadQueryable.Where(cr => studentCourseIds.Select(n => n.StudentId).Contains(cr.StudentId)).ToListAsync(cancellationToken);
            var courseModuleOriginalIds = courses.SelectMany(x => x.CourseModules).Select(n => n.OriginalId);

            var units = await _unitRepository.ReadQueryable.Include(u => u.UnitModules).Where(cr =>
                courseModuleOriginalIds.Contains(cr.OriginalId)
                && cr.VersionStatus == EnumVersionStatus.LastVersion
            ).ToListAsync(cancellationToken);

            var unitResults = await _unitResultRepository.ReadQueryable.Include(p => p.Unit).ThenInclude(p => p.UnitModules).Where(cr => studentCourseIds.Select(n => n.StudentId).Contains(cr.StudentId)).ToListAsync(cancellationToken);
            var unitModuleOriginalIds = units.SelectMany(x => x.UnitModules).Select(n => n.OriginalId);

            var testCourseModules = await _testRepository.ReadQueryable.Include(u => u.TestSections).Where(cr =>
                courseModuleOriginalIds.Contains(cr.OriginalId)
                && cr.VersionStatus == EnumVersionStatus.LastVersion
            ).ToListAsync(cancellationToken);

            var lessons = await _lessonRepository.ReadQueryable.Where(cr =>
                unitModuleOriginalIds.Contains(cr.OriginalId)
                && cr.VersionStatus == EnumVersionStatus.LastVersion
            ).ToListAsync(cancellationToken);

            var lessonResults = await _lessonResultRepository.ReadQueryable
                .Include(p => p.Lesson)
                .Include(p => p.VideoResults)
                .Include(p => p.ClassForumResults)
                .Include(p => p.HomeWorkResults)
                .Include(p => p.DocumentResults)
                .Where(cr => studentCourseIds.Select(n => n.StudentId).Contains(cr.StudentId)).ToListAsync(cancellationToken);

            var testUnitModules = await _testRepository.ReadQueryable.Include(u => u.TestSections).Where(cr =>
                unitModuleOriginalIds.Contains(cr.OriginalId)
                && cr.VersionStatus == EnumVersionStatus.LastVersion
            ).ToListAsync(cancellationToken);

            var testGroupResults = await _testGroupResultRepository.ReadQueryable
                 .Include(x => x.TestResults)
                 .ThenInclude(x => x.Test)
                 .ThenInclude(x => x.TestSections)
                .Where(cr =>
                studentCourseIds.Select(n => n.StudentId).Contains(cr.StudentId ?? Guid.Empty)
            ).ToListAsync(cancellationToken);

            var learningComponents = new List<LearningComponentModel>();
            var tasks = studentCourseIds.Select(async item =>
            {
                using var rootScope = _serviceProvider.CreateScope();

                var course = courses.FirstOrDefault(x => x.Id == item.CourseId);
                var courseResult = courseResults.FirstOrDefault(x => x.StudentId == item.StudentId && x.CourseId == item.CourseId);
                if (course != null)
                {
                    var courseTree = new CourseComponentModel
                    {
                        ComponentName = course.Name,
                        LearningTemplateId = course.Id,
                        LearningOriginalTemplateId = course.OriginalId,
                        LearningResultId = courseResult?.Id,
                        StudentId = item.StudentId,
                        Children = course.CourseModules.Where(p => !item.CourseModuleId.HasValue || p.Id == item.CourseModuleId).Select(cm => new UnitComponentModel
                        {
                            Id = cm.Id,
                            LearningOriginalTemplateId = cm.OriginalId,
                            StudentId = item.StudentId,
                            DisplayOrder = cm.DisplayOrder,
                            Type = cm.CourseConfigType == EnumCourseConfigType.Unit ? EnumCourseConfigType.Unit.ToString() : EnumCourseConfigType.Test.ToString(),
                        } as LearningComponentModel).ToList() ?? new List<LearningComponentModel>(),

                        Units = units,
                        UnitResults = unitResults.Where(x => x.CourseResultId == courseResult?.Id).ToList(),
                        Lessons = lessons,
                        LessonResults = lessonResults.Where(x => x.CourseResultId == courseResult?.Id).ToList(),
                        TestCourseModules = testCourseModules,
                        TestUnitModules = testUnitModules,
                        TestGroupResults = testGroupResults.Where(x => x.CourseResultId == courseResult?.Id).ToList(),
                    };

                    await courseTree.LoadChildren(rootScope.ServiceProvider);
                    learningComponents.Add(courseTree);
                }
            });

            await Task.WhenAll(tasks);

            return learningComponents;
        }
    }

    public abstract class LearningComponentModel
    {
        public Guid Id { get; set; }

        public string? Type { get; set; }

        public string? ComponentName { get; set; }

        public Guid LearningTemplateId { get; set; }

        public Guid LearningOriginalTemplateId { get; set; }

        public Guid? LearningResultId { get; set; }

        public EnumResultStatus? Status { get; set; }

        public Guid StudentId { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public int DisplayOrder { get; set; }

        public int NumberSkill { get; set; }

        public int TotalContent { get; set; }

        public int TotalContentCompleted { get; set; }

        public double? Score { get; set; }

        public string? ScoringFormulaType { get; set; }

        public IList<SkillScores>? SkillScores { get; set; }
        public IList<UnitResult>? UnitResults { get; set; }
        public IList<Unit>? Units { get; set; }
        public IList<TestGroupResult>? TestGroupResults { get; set; }
        public IList<Test>? TestCourseModules { get; set; }
        public IList<Test>? TestUnitModules { get; set; }
        public IList<LessonResult>? LessonResults { get; set; }
        public IList<Lesson>? Lessons { get; set; }

        public List<LearningComponentModel> Children { get; set; } = new List<LearningComponentModel>();

        public IEnumerable<T> GetAllItemByType<T>()
        {
            if (this is T t)
            {
                yield return t;
            }

            foreach (var child in Children)
            {
                foreach (var item in child.GetAllItemByType<T>())
                {
                    yield return item;
                }
            }
        }

        public abstract Task LoadChildren(IServiceProvider serviceProvider);
    }

    public class UnitComponentModel : LearningComponentModel
    {
        public override async Task LoadChildren(IServiceProvider serviceProvider)
        {
            if (Children == null || !Children.Any())
            {
                return;
            }

            using var rootScope = serviceProvider.CreateScope();

            var lessonResultRepository = rootScope.ServiceProvider.GetRequiredService<ILessonResultRepository>();
            var lessonRepository = rootScope.ServiceProvider.GetRequiredService<ILessonRepository>();
            var testGroupResultRepository = rootScope.ServiceProvider.GetRequiredService<ITestGroupResultRepository>();
            var testRepository = rootScope.ServiceProvider.GetRequiredService<ITestRepository>();

            var originalChildrenIds = Children.Select(x => x.LearningOriginalTemplateId).ToList();
            if (!originalChildrenIds.Any())
            {
                return;
            }

            var lessonResults = LearningResultId.HasValue
                ? LessonResults == null
                    ? await lessonResultRepository.ReadQueryable
                        .Where(x => x.StudentId == StudentId && x.UnitResultId == LearningResultId)
                        .Include(x => x.Lesson)
                        .Include(x => x.VideoResults)
                        .Include(x => x.HomeWorkResults)
                        .Include(x => x.ClassForumResults)
                        .Include(x => x.DocumentResults)
                        .ToListAsync()
                    : LessonResults.Where(x => x.StudentId == StudentId && x.UnitResultId == LearningResultId)
                        .ToList()
                : new List<LessonResult>();

            var lessonsHasResult = lessonResults
                .Select(x => x.Lesson)
                .Where(x => x != null)
                .ToList();

            var remainOriginalLessonIds = originalChildrenIds
               .Where(ocid => !lessonsHasResult.Any(u => u != null && u.OriginalId == ocid))
               .ToList();

            var lessons = remainOriginalLessonIds.Any()
                ? Lessons == null
                    ? await lessonRepository.ReadQueryable
                        .WhereBulkContains(remainOriginalLessonIds, x => x.OriginalId)
                        .Where(x => x.VersionStatus == EnumVersionStatus.LastVersion)
                        .ToListAsync()
                    : Lessons
                        .Where(x => remainOriginalLessonIds.Contains(x.OriginalId))
                        .ToList()
                : new List<Lesson>();

            var lessonByOriginal = lessonsHasResult
                .Concat(lessons)
                .GroupBy(x => x.OriginalId)
                .ToDictionary(x => x.Key, x => x.First());

            var lessonResultByLessonId = lessonResults
                .GroupBy(x => x.LessonId)
                .ToDictionary(x => x.Key, x => x.First());

            var tests = TestUnitModules == null
                ? await testRepository.ReadQueryable
                    .Include(x => x.TestSections)
                    .ToListAsync()
                : TestUnitModules.ToList();

            var moduleIds = Children.Select(x => x.Id).ToList();

            var testGroupResults = TestGroupResults == null
                ? await testGroupResultRepository.ReadQueryable
                 .Where(x => x.StudentId == StudentId && x.TestType == EnumTestType.SkillTest && x.UnitModuleId.HasValue && moduleIds.Contains(x.UnitModuleId.Value))
                 .Include(x => x.TestResults)
                 .ThenInclude(x => x.Test)
                 .ThenInclude(x => x.TestSections)
                 .ToListAsync()
                : TestGroupResults
                 .Where(x => x.StudentId == StudentId && x.TestType == EnumTestType.SkillTest && x.UnitModuleId.HasValue && moduleIds.Contains(x.UnitModuleId.Value))
                 .ToList();

            var testByOriginal = tests
                .GroupBy(x => x.OriginalId)
                .ToDictionary(x => x.Key, x => x.First());

            var testResultByTestId = testGroupResults
                .SelectMany(x => x.TestResults)
                .GroupBy(x => x.TestId)
                .ToDictionary(x => x.Key ?? default, x => x.First());

            var testByCourseModule = testGroupResults
                .Where(x => x.CourseModuleId.HasValue)
                .GroupBy(x => x.CourseModuleId!.Value)
                .ToDictionary(x => x.Key, x => x.First());

            foreach (var child in Children)
            {
                if (child.Type == EnumUnitConfigType.Lesson.ToString())
                {
                    if (!lessonByOriginal.TryGetValue(child.LearningOriginalTemplateId, out var lesson))
                        continue;

                    if (child is not LessonComponentModel lessonComponent || lesson == null)
                        continue;

                    lessonResultByLessonId.TryGetValue(lesson.Id, out var result);

                    lessonComponent.ComponentName = lesson.Name;
                    lessonComponent.LearningTemplateId = lesson.Id;
                    lessonComponent.LearningResultId = result?.Id;
                    lessonComponent.Status = result?.Status;
                    lessonComponent.UpdatedDate = result?.CompletionDate ?? result?.UpdatedDate;
                    lessonComponent.TotalContent =
                        lesson.VideoCount + lesson.ClassForumCount + lesson.HomeWorkCount + lesson.DocumentCount;

                    var videoCount = result?.VideoResults.Count(p => p.Status == EnumResultStatus.Done) ?? 0;
                    var classForumCount = result?.ClassForumResults.Count(p => p.ResultStatus == EnumResultStatus.Done) ?? 0;
                    var homeWorkCount = result?.HomeWorkResults.Count(p => p.Status == EnumResultStatus.Done) ?? 0;
                    var documentCount = result?.DocumentResults.Count(p => p.Status == EnumResultStatus.Done) ?? 0;

                    lessonComponent.TotalContentCompleted =
                        videoCount + classForumCount + homeWorkCount + documentCount;
                }
                else
                {
                    Test? test = null;

                    if (testByCourseModule.TryGetValue(child.Id, out var tgr))
                        test = tgr.TestResults.FirstOrDefault()?.Test;
                    else
                        testByOriginal.TryGetValue(child.LearningOriginalTemplateId, out test);

                    if (test == null || child is not LessonComponentModel testComponent)
                        continue;

                    testResultByTestId.TryGetValue(test.Id, out var testResult);

                    testComponent.ComponentName = test.Name;
                    testComponent.LearningTemplateId = test.Id;
                    testComponent.LearningResultId = testResult?.Id;
                    testComponent.Status = testResult?.Status;
                    testComponent.UpdatedDate = testResult?.CompletionDate ?? testResult?.UpdatedDate;
                    testComponent.NumberSkill = test.TestSections.Count(x => !x.ParentId.HasValue);
                    testComponent.Score = testResult?.Score;
                    testComponent.SkillScores = testResult?.SkillScores;
                }
            }
        }
    }

    public class LessonComponentModel : LearningComponentModel
    {
        public override Task LoadChildren(IServiceProvider serviceProvider)
        {
            return Task.CompletedTask;
        }
    }

    public class CourseComponentModel : LearningComponentModel
    {
        public override async Task LoadChildren(IServiceProvider serviceProvider)
        {
            if (Children == null || !Children.Any())
            {
                return;
            }

            using var rootScope = serviceProvider.CreateScope();

            var unitResultRepository = rootScope.ServiceProvider.GetRequiredService<IUnitResultRepository>();
            var unitRepository = rootScope.ServiceProvider.GetRequiredService<IUnitRepository>();
            var testRepository = rootScope.ServiceProvider.GetRequiredService<ITestRepository>();
            var testGroupResultRepository = rootScope.ServiceProvider.GetRequiredService<ITestGroupResultRepository>();

            var originalChildrenIds = Children.Select(x => x.LearningOriginalTemplateId).ToList();

            if (!originalChildrenIds.Any())
            {
                return;
            }

            var unitResults = LearningResultId.HasValue
                ? UnitResults == null
                    ? await unitResultRepository.ReadQueryable
                        .Where(x => x.StudentId == StudentId && x.CourseResultId == LearningResultId)
                        .Include(x => x.Unit)
                        .ThenInclude(u => u.UnitModules)
                        .ToListAsync()
                    : UnitResults
                        .Where(x => x.StudentId == StudentId && x.CourseResultId == LearningResultId)
                        .ToList()
                : new List<UnitResult>();

            var unitsHasResult = unitResults.Select(x => x.Unit).Where(x => x != null).ToList();

            var remainOriginalUnitIds = originalChildrenIds
                .Where(ocid => !unitsHasResult.Any(u => u != null && u.OriginalId == ocid))
                .ToList();

            var units = remainOriginalUnitIds.Any()
                ? Units == null
                    ? await unitRepository.ReadQueryable
                        .Where(u => remainOriginalUnitIds.Contains(u.OriginalId)
                                    && u.VersionStatus == EnumVersionStatus.LastVersion)
                        .Include(u => u.UnitModules)
                        .ToListAsync()
                    : Units
                        .Where(u => remainOriginalUnitIds.Contains(u.OriginalId)
                                    && u.VersionStatus == EnumVersionStatus.LastVersion)
                        .ToList()
                : new List<Unit>();

            var unitByOriginal = unitsHasResult
                .Concat(units)
                .GroupBy(x => x.OriginalId)
                .ToDictionary(x => x.Key, x => x.First());

            var unitResultByUnitId = unitResults
                .GroupBy(x => x.UnitId)
                .ToDictionary(x => x.Key, x => x.First());

            var tests = TestCourseModules == null ?
                await testRepository.ReadQueryable
                .Include(x => x.TestSections)
                .ToListAsync()
                : TestCourseModules
                .ToList();

            var moduleIds = Children.Select(x => x.Id).ToList();

            var testGroupResults = TestGroupResults == null ?
                await testGroupResultRepository.ReadQueryable
                    .Where(x => x.StudentId == StudentId && x.TestType == EnumTestType.FullTest && x.CourseModuleId.HasValue && moduleIds.Contains(x.CourseModuleId.Value))
                    .Include(x => x.TestResults)
                    .ThenInclude(x => x.Test)
                    .ThenInclude(x => x.TestSections)
                    .ToListAsync()
                : TestGroupResults
                    .Where(x => x.StudentId == StudentId && x.TestType == EnumTestType.FullTest && x.CourseModuleId.HasValue && moduleIds.Contains(x.CourseModuleId.Value))
                    .ToList();

            var testByOriginal = tests
                .GroupBy(x => x.OriginalId)
                .ToDictionary(x => x.Key, x => x.First());

            var testResultByTestId = testGroupResults
                .SelectMany(x => x.TestResults)
                .GroupBy(x => x.TestId)
                .ToDictionary(x => x.Key ?? default, x => x.First());

            var testByCourseModule = testGroupResults
                .Where(x => x.CourseModuleId.HasValue)
                .GroupBy(x => x.CourseModuleId!.Value)
                .ToDictionary(x => x.Key, x => x.First());

            var tasks = Children.Select(async child =>
            {
                using var scope = serviceProvider.CreateScope();
                var scopedProvider = scope.ServiceProvider;

                if (child.Type == EnumCourseConfigType.Unit.ToString())
                {
                    if (!unitByOriginal.TryGetValue(child.LearningOriginalTemplateId, out var unit))
                    {
                        return;
                    }

                    if (child is not UnitComponentModel unitComponent || unit == null)
                    {
                        return;
                    }

                    unitResultByUnitId.TryGetValue(unit.Id, out var result);

                    unitComponent.ComponentName = unit.Name;
                    unitComponent.LearningTemplateId = unit.Id;
                    unitComponent.LearningResultId = result?.Id;
                    unitComponent.Status = result?.Status;
                    unitComponent.UpdatedDate = result?.CompletionDate ?? result?.UpdatedDate;

                    unitComponent.Children = unit.UnitModules
                        .OrderBy(x => x.DisplayOrder)
                        .Select(um => new LessonComponentModel
                        {
                            Id = um.Id,
                            LearningOriginalTemplateId = um.OriginalId,
                            StudentId = StudentId,
                            DisplayOrder = um.DisplayOrder,
                            Type = um.UnitConfigType == EnumUnitConfigType.Lesson
                                ? EnumUnitConfigType.Lesson.ToString()
                                : EnumUnitConfigType.Test.ToString()
                        })
                        .Cast<LearningComponentModel>()
                        .ToList();

                    unitComponent.Lessons = Lessons;
                    unitComponent.LessonResults = LessonResults?.Where(x => x.UnitResultId == result?.Id).ToList();
                    unitComponent.TestUnitModules = TestUnitModules;
                    unitComponent.TestGroupResults = testGroupResults.Where(x => x.UnitResultId == result?.Id).ToList();

                    await unitComponent.LoadChildren(scopedProvider);
                }
                else
                {
                    Test? test = null;

                    if (testByCourseModule.TryGetValue(child.Id, out var tgr))
                    {
                        test = tgr.TestResults.FirstOrDefault()?.Test;
                    }
                    else
                    {
                        testByOriginal.TryGetValue(child.LearningOriginalTemplateId, out test);
                    }

                    if (test == null || child is not UnitComponentModel testComponent)
                    {
                        return;
                    }

                    testResultByTestId.TryGetValue(test.Id, out var testResult);

                    testComponent.ComponentName = test.Name;
                    testComponent.LearningTemplateId = test.Id;
                    testComponent.LearningResultId = testResult?.Id;
                    testComponent.Status = testResult?.Status;
                    testComponent.UpdatedDate = testResult?.CompletionDate ?? testResult?.UpdatedDate;
                    testComponent.NumberSkill = test.TestSections.Count(x => !x.ParentId.HasValue);
                    testComponent.SkillScores = testResult?.SkillScores;
                    testComponent.ScoringFormulaType = test.ScoringFormulaType.ToString();
                }
            });

            await Task.WhenAll(tasks);
        }
    }
}
