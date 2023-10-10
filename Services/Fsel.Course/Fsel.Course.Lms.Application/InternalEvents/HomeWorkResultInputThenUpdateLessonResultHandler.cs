// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.InternalEvents
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Core.Applications.InternalEvents;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Course.Lms.Application.Services.SenderService;
    using Fsel.Course.Lms.Application.Services.TrainingServices;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class HomeWorkResultInputThenUpdateLessonResultHandler : BaseInternalLessonResultEventHandler,
        INotificationHandler<EntityChangedEvent<HomeWorkResult>>
    {
        public HomeWorkResultInputThenUpdateLessonResultHandler(ITrainingService trainingService, ICourseUnitMockTestRepository courseUnitMockTestRepository, IMediator mediator, IUserService userService, ISenderService senderService, IVideoResultRepository videoResultRepository, IClassForumResultRepository classForumResultRepository, IUnitResultRepository unitResultRepository, ILessonResultRepository lessonResultRepository, ICourseResultRepository courseResultRepository, ICourseRepository courseRepository, IUnitRepository unitRepository, IMockTestRepository mockTestRepository, IHomeWorkQuestionRepository homeWorkQuestionRepository, IHomeWorkAnswerRepository homeWorkAnswerRepository, IQuestionRepository questionRepository, IHomeWorkRepository homeWorkRepository, FinishOneUnitPublisher finishOneUnitPublisher, FinishOneLevelPassPublisher finishOneLevelPassPublisher, IFinalTestResultRepository finalTestResultRepository, IMockTestResultRepository mockTestResultRepository, IHomeWorkResultRepository homeWorkResultRepository) : base(trainingService, courseUnitMockTestRepository, mediator, userService, senderService, videoResultRepository, classForumResultRepository, unitResultRepository, lessonResultRepository, courseResultRepository, courseRepository, unitRepository, mockTestRepository, homeWorkQuestionRepository, homeWorkAnswerRepository, questionRepository, homeWorkRepository, finishOneUnitPublisher, finishOneLevelPassPublisher, finalTestResultRepository, mockTestResultRepository, homeWorkResultRepository)
        {
        }

        public async Task Handle(EntityChangedEvent<HomeWorkResult> notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
            var homeWorkResult = notification.Data;
            var lessonResult = await _lessonResultRepository.Queryable.Include(x => x.HomeWorkResults.Where(x => x.StudentId == homeWorkResult.StudentId && x.LessonResultId == homeWorkResult.LessonResultId))
                                                                        .Include(x => x.ClassForumResults.Where(x => x.StudentId == homeWorkResult.StudentId && x.LessonResultId == homeWorkResult.LessonResultId))
                                                                        .FirstOrDefaultAsync(x => x.Id == homeWorkResult.LessonResultId, cancellationToken);
            if (lessonResult != null)
            {
                var isHomeWorksDone = lessonResult.HomeWorkResults.All(x => x.StudentId == homeWorkResult.StudentId && x.LessonResultId == homeWorkResult.LessonResultId && x.Status == EnumResultStatus.Done);
                var isClassForumDone = lessonResult.ClassForumResults.Any(x => x.StudentId == homeWorkResult.StudentId && x.LessonResultId == homeWorkResult.LessonResultId && (x.Status == EnumClassForumResultStatus.PendingForGrading || x.Status == EnumClassForumResultStatus.Graded));
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
    }
}
