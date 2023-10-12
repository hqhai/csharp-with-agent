// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.InternalEvents
{
    using System.Linq.Dynamic.Core;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Core.Applications.InternalEvents;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Infrastructure.ValueSettings;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class ClassForumResultInputThenUpdateLessonResultHandler : BaseInternalLessonResultEventHandler,
        INotificationHandler<EntityChangedEvent<ClassForumResult>>, INotificationHandler<EntityCreatedEvent<ClassForumResult>>
    {
        private readonly ILessonResultRepository _lessonResultRepository;

        public ClassForumResultInputThenUpdateLessonResultHandler(ISystemService systemService, AppSetting appSetting, FinishOneLevelPassPublisher finishOneLevelPassPublisher, ICourseUnitMockTestRepository courseUnitMockTestRepository, IMediator mediator, IUserService userService, IVideoResultRepository videoResultRepository, IClassForumResultRepository classForumResultRepository, IUnitResultRepository unitResultRepository, ICourseResultRepository courseResultRepository, ICourseRepository courseRepository, IUnitRepository unitRepository, IFinalTestResultRepository finalTestResultRepository, IMockTestResultRepository mockTestResultRepository, IHomeWorkResultRepository homeWorkResultRepository, ILessonResultRepository lessonResultRepository) : base(systemService, appSetting, finishOneLevelPassPublisher, courseUnitMockTestRepository, mediator, userService, videoResultRepository, classForumResultRepository, unitResultRepository, courseResultRepository, courseRepository, unitRepository, finalTestResultRepository, mockTestResultRepository, homeWorkResultRepository)
        {
            _lessonResultRepository = lessonResultRepository;
        }

        public async Task Handle(EntityChangedEvent<ClassForumResult> notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
            await ExecuteEvent(notification.Data, cancellationToken);
        }

        public async Task Handle(EntityCreatedEvent<ClassForumResult> notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
            await ExecuteEvent(notification.Data, cancellationToken);
        }

        private async Task ExecuteEvent(ClassForumResult classForumResult, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(classForumResult);
            var lessonResult = await _lessonResultRepository.Queryable.Include(x => x.HomeWorkResults.Where(x => x.StudentId == classForumResult.StudentId && x.LessonResultId == classForumResult.LessonResultId))
                                                                         .Include(x => x.ClassForumResults.Where(x => x.StudentId == classForumResult.StudentId && x.LessonResultId == classForumResult.LessonResultId))
                                                                         .FirstOrDefaultAsync(x => x.Id == classForumResult.LessonResultId, cancellationToken);
            if (lessonResult != null)
            {
                if (classForumResult.Status == EnumClassForumResultStatus.Pending && lessonResult.ClassForumResults.Count == 1)
                {
                    await UpdateHomeWorks(classForumResult, cancellationToken).ConfigureAwait(false);
                }
                var isHomeWorksDone = lessonResult.HomeWorkResults.All(x => x.StudentId == classForumResult.StudentId && x.LessonResultId == classForumResult.LessonResultId && x.Status == EnumResultStatus.Done);
                var isClassForumDone = lessonResult.ClassForumResults.Any(x => x.StudentId == classForumResult.StudentId && x.LessonResultId == classForumResult.LessonResultId && (x.Status == EnumClassForumResultStatus.PendingForGrading || x.Status == EnumClassForumResultStatus.Graded));
                await GetLessonResult(lessonResult, cancellationToken);
                if (isClassForumDone && isHomeWorksDone)
                {
                    lessonResult.Status = EnumResultStatus.Done;
                    _lessonResultRepository.Update(lessonResult);
                    await _lessonResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    _lessonResultRepository.Update(lessonResult);
                    await _lessonResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }
            }
        }

        private async Task UpdateHomeWorks(ClassForumResult classForumResult, CancellationToken cancellationToken)
        {
            var homeWorkResults = await _homeWorkResultRepository.Queryable.Where(x => x.LessonResultId == classForumResult.LessonResultId && x.Status == EnumResultStatus.Unfinished).ToListAsync(cancellationToken);
            if (homeWorkResults != null && homeWorkResults.Any())
            {
                homeWorkResults = homeWorkResults.Select(x => { x.Status = EnumResultStatus.New; return x; }).ToList();
                _homeWorkResultRepository.UpdateList(homeWorkResults);
                await _lessonResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
