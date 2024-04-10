// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.AiCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.ClassForumAutoDot;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class SubmitClassforumAICommand : ClassForumAIResponseModel, IRequest<bool>
    {
    }

    public class SubmitAIResponseCommandHandler : IRequestHandler<SubmitClassforumAICommand, bool>
    {
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly SubmitAIResponsePublisher _submitAIResponsePublisher;
        private readonly NotificationMessagePublisher _notificationMessagePublisher;
        private readonly IMediator _mediator;
        private readonly IClassforumDetailResultRepository _classforumDetailResultRepository;

        public SubmitAIResponseCommandHandler(ILessonResultRepository lessonResultRepository, SubmitAIResponsePublisher submitAIResponsePublisher, NotificationMessagePublisher notificationMessagePublisher, IMediator mediator, IClassforumDetailResultRepository classforumDetailResultRepository)
        {
            _lessonResultRepository = lessonResultRepository;
            _submitAIResponsePublisher = submitAIResponsePublisher;
            _notificationMessagePublisher = notificationMessagePublisher;
            _mediator = mediator;
            _classforumDetailResultRepository = classforumDetailResultRepository;
        }

        public async Task<bool> Handle(SubmitClassforumAICommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var classForumDetailResult = await _classforumDetailResultRepository.GetByIdAsync(request.ClassForumDetailResultId);

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

            if (classForumDetailResult != null)
            {
                if (request.IsRetry != null && (bool)request.IsRetry)
                {
                }
                else
                {
                    var classForumAIs = ConvertHelper.Deserialize<List<ClassForumAIModel>>(aIResponse);
                    classForumDetailResult.GradingAlFeedback = ConvertHelper.Serialize(classForumAIs);
                }

                _classforumDetailResultRepository.Update(classForumDetailResult);
                await _classforumDetailResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            await _submitAIResponsePublisher.Publish(new SubmitAIResponseModel
            {
                GradingAlFeedback = aIResponse,
                ClassForumResultId = request.ClassForumDetailResultId,
            }, cancellationToken);

            if (!string.IsNullOrEmpty(aIResponse))
            {
                var classForumDetailResultOwner = _classforumDetailResultRepository.Queryable.Include(x => x.ClassForumResult).FirstOrDefault(x => x.Id == request.ClassForumDetailResultId);

                if (classForumDetailResultOwner != null)
                {
                    var lessonResult = await _lessonResultRepository.GetIncludeByIdAsync(classForumDetailResultOwner.ClassForumResult!.LessonResultId);
                    var paramsLink = new List<object> { lessonResult?.LessonId.ToString() ?? string.Empty, lessonResult?.CourseId.ToString() ?? string.Empty, lessonResult?.UnitId.ToString() ?? string.Empty };

                    NotificationSendingQueueModel notificationQueue = new NotificationSendingQueueModel()
                    {
                        ObjectId = classForumDetailResultOwner.Id,
                        UserIds = new List<Guid>() { classForumDetailResultOwner.CreatedUserId },
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
