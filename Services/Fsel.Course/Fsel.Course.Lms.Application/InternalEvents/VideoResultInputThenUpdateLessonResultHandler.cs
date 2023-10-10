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
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class VideoResultInputThenUpdateLessonResultHandler : BaseInternalLessonResultEventHandler,
        INotificationHandler<EntityChangedEvent<VideoResult>>
    {
        private readonly FinishOneLessonPublisher _finishOneLessonPublisher;

        public VideoResultInputThenUpdateLessonResultHandler(FinishOneLessonPublisher finishOneLessonPublisher, ITrainingService trainingService, ICourseUnitMockTestRepository courseUnitMockTestRepository, IMediator mediator, IUserService userService, ISenderService senderService, IVideoResultRepository videoResultRepository, IClassForumResultRepository classForumResultRepository, IUnitResultRepository unitResultRepository, ILessonResultRepository lessonResultRepository, ICourseResultRepository courseResultRepository, ICourseRepository courseRepository, IUnitRepository unitRepository, IMockTestRepository mockTestRepository, IHomeWorkQuestionRepository homeWorkQuestionRepository, IHomeWorkAnswerRepository homeWorkAnswerRepository, IQuestionRepository questionRepository, IHomeWorkRepository homeWorkRepository, FinishOneUnitPublisher finishOneUnitPublisher, FinishOneLevelPassPublisher finishOneLevelPassPublisher, IFinalTestResultRepository finalTestResultRepository, IMockTestResultRepository mockTestResultRepository, IHomeWorkResultRepository homeWorkResultRepository) : base(trainingService, courseUnitMockTestRepository, mediator, userService, senderService, videoResultRepository, classForumResultRepository, unitResultRepository, lessonResultRepository, courseResultRepository, courseRepository, unitRepository, mockTestRepository, homeWorkQuestionRepository, homeWorkAnswerRepository, questionRepository, homeWorkRepository, finishOneUnitPublisher, finishOneLevelPassPublisher, finalTestResultRepository, mockTestResultRepository, homeWorkResultRepository)
        {
            _finishOneLessonPublisher = finishOneLessonPublisher;
        }

        public async Task Handle(EntityChangedEvent<VideoResult> notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
            var videoResult = notification.Data;
            var lessonResult = await _lessonResultRepository.Queryable.FirstOrDefaultAsync(x => x.Id == videoResult.LessonResultId, cancellationToken);
            if (lessonResult != null && videoResult.Status == EnumResultStatus.Done)
            {
                var skillScores = videoResult.VideoSkillScores?.FirstOrDefault(x => x.Type == EnumTimeCodeType.Standalone)?.SkillScores;
                lessonResult.CorrectCount = videoResult.CorrectCount;
                lessonResult.CorrectTotal = videoResult.CorrectTotal;
                lessonResult.Percent = NumberHelper.ConvertDoublePercent(videoResult.Percent * 40);
                lessonResult.SkillScores = skillScores;
                await _finishOneLessonPublisher.Publish(lessonResult, cancellationToken);
                await GetLessonResult(lessonResult, cancellationToken);
                _lessonResultRepository.Update(lessonResult);
                await _lessonResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
