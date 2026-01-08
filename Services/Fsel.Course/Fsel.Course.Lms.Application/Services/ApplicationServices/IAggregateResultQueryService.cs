// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Fsel.Common.Enums;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;

    public interface IAggregateResultQueryService
    {
        Task<LearningComponent> GetLearningTreeFromCourseToLesson(Guid studentId, Guid courseResultId, CancellationToken cancellationToken = default);
    }

    public class AggregateResultQueryService : IAggregateResultQueryService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ICourseResultRepository _courseResultRepository;

        public AggregateResultQueryService(IServiceProvider serviceProvider,
            ICourseResultRepository courseResultRepository)
        {
            _serviceProvider = serviceProvider;
            _courseResultRepository = courseResultRepository;
        }

        public async Task<LearningComponent> GetLearningTreeFromCourseToLesson(Guid studentId, Guid courseResultId, CancellationToken cancellationToken = default)
        {
            var courseResult = await _courseResultRepository.ReadQueryable.Where(cr => cr.Id == courseResultId)
                .Include(cr => cr.Course)
                .ThenInclude(u => u.CourseModules)
                .FirstOrDefaultAsync(cancellationToken);

            if (courseResult == null)
            {
                return null;
            }

            var courseTree = new CourseComponent
            {
                ComponentName = courseResult.Course?.GetType().Name,
                LearningTemplateId = courseResult.Course?.Id ?? Guid.Empty,
                LearningResultId = courseResultId,
                StudentId = studentId,
                Children = courseResult.Course?.CourseModules.Where(x => x.CourseConfigType == EnumCourseConfigType.Unit).Select(cm => new UnitComponent
                {
                    LearningOriginalTemplateId = cm.OriginalId,
                    StudentId = studentId
                } as LearningComponent).ToList() ?? new List<LearningComponent>(),
            };

            await courseTree.LoadChildren(_serviceProvider);

            return courseTree;
        }
    }

    public abstract class LearningComponent
    {
        public string? ComponentName { get; set; }

        public Guid LearningTemplateId { get; set; }

        public Guid LearningOriginalTemplateId { get; set; }

        public Guid? LearningResultId { get; set; }

        public EnumResultStatus? Status { get; set; }

        public Guid StudentId { get; set; }

        public List<LearningComponent> Children { get; set; } = new List<LearningComponent>();

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

    public class UnitComponent : LearningComponent
    {
        public override async Task LoadChildren(IServiceProvider serviceProvider)
        {
            if (Children == null || !Children.Any())
            {
                return;
            }

            var lessonResultRepository = serviceProvider.GetRequiredService<ILessonResultRepository>();
            var lessonRepository = serviceProvider.GetRequiredService<ILessonRepository>();

            var originalChildrenIds = Children.Select(x => x.LearningOriginalTemplateId).ToList();

            if (originalChildrenIds == null || !originalChildrenIds.Any())
            {
                return;
            }

            var lessonResults = LearningResultId.HasValue ? await lessonResultRepository.ReadQueryable
                .Where(ur => ur.StudentId == StudentId && ur.UnitResultId == LearningResultId)
                .Include(ur => ur.Lesson)
                .ToListAsync() : new List<LessonResult>();

            var lessonsHasResult = lessonResults.Select(x => x.Lesson).ToList();

            foreach (var child in Children)
            {
                var lesson = lessonsHasResult.FirstOrDefault(ur => ur.OriginalId == child.LearningOriginalTemplateId);
                if (lesson != null && child is LessonComponent lessonComponent)
                {
                    var result = lessonResults.FirstOrDefault(ur => ur.LessonId == lesson.Id);
                    lessonComponent.ComponentName = lesson.Name;
                    lessonComponent.LearningTemplateId = lesson.Id;
                    lessonComponent.LearningResultId = result?.Id;
                    lessonComponent.Status = result?.Status;
                }
            }
        }
    }

    public class LessonComponent : LearningComponent
    {
        public override Task LoadChildren(IServiceProvider serviceProvider)
        {
            return Task.CompletedTask;
        }
    }

    public class CourseComponent : LearningComponent
    {
        public override async Task LoadChildren(IServiceProvider serviceProvider)
        {
            if (Children == null || !Children.Any())
            {
                return;
            }

            var unitResultRepository = serviceProvider.GetRequiredService<IUnitResultRepository>();
            var unitRepository = serviceProvider.GetRequiredService<IUnitRepository>();

            var originalChildrenIds = Children.Select(x => x.LearningOriginalTemplateId).ToList();

            if (originalChildrenIds == null || !originalChildrenIds.Any())
            {
                return;
            }

            var unitResults = LearningResultId.HasValue ? await unitResultRepository.ReadQueryable
                .Where(ur => ur.StudentId == StudentId && ur.CourseResultId == LearningResultId)
                .Include(ur => ur.Unit)
                .ThenInclude(u => u.UnitModules)
                .ToListAsync() : new List<UnitResult>();

            var unitsHasResult = unitResults.Select(x => x.Unit).ToList();

            var remainOriginalUnitIds = originalChildrenIds.Where(ocid => !unitsHasResult.Any(ur => ur.OriginalId == ocid)).ToList();

            var units = remainOriginalUnitIds.Count > 0 ? await unitRepository.ReadQueryable
                .Where(u => remainOriginalUnitIds.Contains(u.OriginalId) && u.VersionStatus == EnumVersionStatus.LastVersion)
                .Include(u => u.UnitModules)
                .ToListAsync() : new List<Unit>();

            foreach (var child in Children)
            {
                var unit = unitsHasResult.FirstOrDefault(ur => ur.OriginalId == child.LearningOriginalTemplateId)
                    ?? units.FirstOrDefault(u => u.OriginalId == child.LearningOriginalTemplateId);
                if (unit != null && child is UnitComponent unitComponent)
                {
                    var result = unitResults.FirstOrDefault(ur => ur.UnitId == unit.Id);
                    unitComponent.ComponentName = unit.Name;
                    unitComponent.LearningTemplateId = unit.Id;
                    unitComponent.LearningResultId = result?.Id;
                    unitComponent.Status = result?.Status;
                    unitComponent.Children = unit.UnitModules.Where(um => um.UnitConfigType == EnumUnitConfigType.Lesson).Select(um => new LessonComponent
                    {
                        LearningOriginalTemplateId = um.OriginalId,
                        StudentId = StudentId
                    } as LearningComponent).ToList();
                    await unitComponent.LoadChildren(serviceProvider);
                }
            }
        }
    }
}
