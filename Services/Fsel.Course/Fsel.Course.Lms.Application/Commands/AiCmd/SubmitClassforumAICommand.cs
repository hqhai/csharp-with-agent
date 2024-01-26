// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.AiCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.QueryModels.ClassForumAutoDot;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;

    public class SubmitClassforumAICommand : ClassForumAIResponseModel, IRequest<bool>
    {
    }

    public class SubmitAIResponseCommandHandler : IRequestHandler<SubmitClassforumAICommand, bool>
    {
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly SubmitAIResponsePublisher _submitAIResponsePublisher;
        private readonly NotificationMessagePublisher _notificationMessagePublisher;
        private readonly IMediator _mediator;
        public SubmitAIResponseCommandHandler(IClassForumResultRepository classForumResultRepository, ILessonResultRepository lessonResultRepository, SubmitAIResponsePublisher submitAIResponsePublisher, IMediator mediator, NotificationMessagePublisher notificationMessagePublisher)
        {
            _classForumResultRepository = classForumResultRepository;
            _lessonResultRepository = lessonResultRepository;
            _submitAIResponsePublisher = submitAIResponsePublisher;
            _mediator = mediator;
            _notificationMessagePublisher = notificationMessagePublisher;
        }

        public async Task<bool> Handle(SubmitClassforumAICommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var classForumResult = await _classForumResultRepository.GetByIdAsync(request.ClassForumResultId);
            var userAiConfig = request!.UserAIConfig?.Replace("{0}", request.WordContent, StringComparison.CurrentCulture);
            var aIResponse = await _mediator.Send(new SubmitAICommand
            {
                SettingModel = request.SettingModel,
                SettingTemperature = request.SettingTemperature,
                SettingFrequecy = request.SettingFrequecy,
                SettingWordMaxLength = request.SettingWordMaxLength,
                SettingPresence = request.SettingPresence,
                SettingTopP = request.SettingTopP,
                SystemRoleAlConfig = request.SystemRoleAlConfig,
                UserAIConfig = userAiConfig,
            }, cancellationToken).ConfigureAwait(false);


            if (classForumResult != null)
            {
                if (request.IsRetry != null && (bool)request.IsRetry)
                {
                    classForumResult.RetryGradingAlFeedBack = aIResponse;
                }
                else
                {
                    classForumResult.GradingAlFeedback = aIResponse;

                }


                _classForumResultRepository.Update(classForumResult);
                await _classForumResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            }

            await _submitAIResponsePublisher.Publish(new SubmitAIResponseModel
            {
                GradingAlFeedback = aIResponse,
                ClassForumResultId = request.ClassForumResultId,
            }, cancellationToken);


            if (!string.IsNullOrEmpty(aIResponse))
            {
                var classForumResultOwner = _classForumResultRepository.Queryable.FirstOrDefault(x => x.Id == request.ClassForumResultId);

                if (classForumResultOwner != null)
                {
                    var lessonResult = await _lessonResultRepository.GetIncludeByIdAsync(classForumResultOwner.LessonResultId);
                    var paramsLink = new List<object> { lessonResult?.LessonId.ToString() ?? string.Empty, lessonResult?.CourseId.ToString() ?? string.Empty, lessonResult?.UnitId.ToString() ?? string.Empty };

                    NotificationSendingQueueModel notificationQueue = new NotificationSendingQueueModel()
                    {
                        ObjectId = classForumResultOwner.Id,
                        UserIds = new List<Guid>() { classForumResultOwner.CreatedUserId },
                        Type = EnumNotificationType.LinkPage,
                        Content = EnumNotificationContent.AIFeedBack,
                        PlatformCode = EnumPlatformCode.LMS,
                        ParamsLink = paramsLink
                    };
                    await _notificationMessagePublisher.Publish(notificationQueue, cancellationToken);
                }

            }
            return true;
        }
    }
}
