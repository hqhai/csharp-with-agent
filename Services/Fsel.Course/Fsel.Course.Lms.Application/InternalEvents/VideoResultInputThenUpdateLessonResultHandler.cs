// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.InternalEvents
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Core.Applications.InternalEvents;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Infrastructure.ValueSettings;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Course.Lms.Application.Services.OrderServices;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.Extensions.Logging;

    public class VideoResultInputThenUpdateLessonResultHandler : BaseInternalLessonResultEventHandler,
        INotificationHandler<EntityChangedEvent<VideoResult>>
    {
        private readonly ILogger<VideoResultInputThenUpdateLessonResultHandler> _logger2;
        private readonly ILessonResultRepository _lessonResultRepository;

        public VideoResultInputThenUpdateLessonResultHandler(ISystemService systemService, ILogger<VideoResultInputThenUpdateLessonResultHandler> logger2, AppSetting appSetting, ILogger<BaseInternalLessonResultEventHandler> logger1, ICourseUnitMockTestRepository courseUnitMockTestRepository, IMediator mediator, IUserService userService, ILogger<BaseInternalEventHandler> logger, SaveUserCourseSettingPublisher saveUserCourseSettingPublisher, IVideoResultRepository videoResultRepository, IClassForumResultRepository classForumResultRepository, IUnitResultRepository unitResultRepository, ICourseResultRepository courseResultRepository, ICourseRepository courseRepository, IUnitRepository unitRepository, IFinalTestResultRepository finalTestResultRepository, IMockTestResultRepository mockTestResultRepository, IHomeWorkResultRepository homeWorkResultRepository, QuestBoardPublisher questBoardPublisher, IOrderService orderService, ILessonNoteRepository lessonNoteRepository, ILessonResultRepository lessonResultRepository) : base(systemService, appSetting, logger1, courseUnitMockTestRepository, mediator, userService, logger, saveUserCourseSettingPublisher, videoResultRepository, classForumResultRepository, unitResultRepository, courseResultRepository, courseRepository, unitRepository, finalTestResultRepository, mockTestResultRepository, homeWorkResultRepository, questBoardPublisher, orderService, lessonNoteRepository, lessonResultRepository)
        {
            _logger2 = logger2;
            _lessonResultRepository = lessonResultRepository;
        }

        public async Task Handle(EntityChangedEvent<VideoResult> notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
            var videoResult = notification.Data;
            try
            {
                var lessonResult = videoResult.LessonResult;
                if (lessonResult == null || videoResult.Status != EnumResultStatus.Done)
                {
                    return;
                }
                var skillScores = videoResult.VideoSkillScores?.FirstOrDefault(x => x.Type == EnumTimeCodeType.Standalone)?.SkillScores;
                lessonResult.CorrectCount = videoResult.CorrectCount;
                lessonResult.CorrectTotal = videoResult.CorrectTotal;
                lessonResult.Percent = NumberHelper.ConvertDoublePercent(videoResult.Percent * 40);
                lessonResult.SkillScores = skillScores;
                _lessonResultRepository.Update(lessonResult);
                await _lessonResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger2.LogWarning($"Log Trigger VideoResult : {ex.Message} ");
            }
        }
    }
}
