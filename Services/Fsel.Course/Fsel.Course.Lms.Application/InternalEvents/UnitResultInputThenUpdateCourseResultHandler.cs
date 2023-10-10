// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.InternalEvents
{
    using Fsel.Core.Applications.InternalEvents;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Course.Lms.Application.Services.SenderService;
    using Fsel.Course.Lms.Application.Services.TrainingServices;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;

    public class UnitResultInputThenUpdateCourseResultHandler : BaseInternalEventHandler,
        INotificationHandler<EntityChangedEvent<UnitResult>>
    {
        public UnitResultInputThenUpdateCourseResultHandler(ITrainingService trainingService, ICourseUnitMockTestRepository courseUnitMockTestRepository, IMediator mediator, IUserService userService, ISenderService senderService, IVideoResultRepository videoResultRepository, IClassForumResultRepository classForumResultRepository, IUnitResultRepository unitResultRepository, ILessonResultRepository lessonResultRepository, ICourseResultRepository courseResultRepository, ICourseRepository courseRepository, IUnitRepository unitRepository, IMockTestRepository mockTestRepository, IHomeWorkQuestionRepository homeWorkQuestionRepository, IHomeWorkAnswerRepository homeWorkAnswerRepository, IQuestionRepository questionRepository, IHomeWorkRepository homeWorkRepository, FinishOneUnitPublisher finishOneUnitPublisher, FinishOneLevelPassPublisher finishOneLevelPassPublisher, IFinalTestResultRepository finalTestResultRepository, IMockTestResultRepository mockTestResultRepository, IHomeWorkResultRepository homeWorkResultRepository) : base(trainingService, courseUnitMockTestRepository, mediator, userService, senderService, videoResultRepository, classForumResultRepository, unitResultRepository, lessonResultRepository, courseResultRepository, courseRepository, unitRepository, mockTestRepository, homeWorkQuestionRepository, homeWorkAnswerRepository, questionRepository, homeWorkRepository, finishOneUnitPublisher, finishOneLevelPassPublisher, finalTestResultRepository, mockTestResultRepository, homeWorkResultRepository)
        {
        }

        public async Task Handle(EntityChangedEvent<UnitResult> notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
            var unitResult = notification.Data;
            await UpdateCourse(unitResult.CourseId, unitResult.StudentId, cancellationToken).ConfigureAwait(false);
            if (unitResult.Status == EnumResultStatus.Done)
            {
                await UpdateProcessUnit(unitResult, cancellationToken);
            }
        }
    }
}
