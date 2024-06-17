// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.AiCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.ClassForumAutoDot;
    using Fsel.Course.Infrastructure.ValueSettings;
    using Fsel.Course.Lms.Application.Commands.SenderCmd;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using Kros.Extensions;
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
        private readonly IMapper _mapper;
        private readonly AppSetting _appSetting;
        private readonly IClassForumDetailResultRepository _classForumDetailResultRepository;
        private readonly SetTimeRetryClassForumPublisher _setTimeRetryClassForumPublisher;
        private const int Max_Time_Retry = 3;
        public SubmitAIResponseCommandHandler(ILessonResultRepository lessonResultRepository, SubmitAIResponsePublisher submitAIResponsePublisher, NotificationMessagePublisher notificationMessagePublisher, IMediator mediator, IClassForumDetailResultRepository classForumDetailResultRepository, SetTimeRetryClassForumPublisher setTimeRetryClassForumPublisher, IMapper mapper, AppSetting appSetting)
        {
            _lessonResultRepository = lessonResultRepository;
            _submitAIResponsePublisher = submitAIResponsePublisher;
            _notificationMessagePublisher = notificationMessagePublisher;
            _mediator = mediator;
            _classForumDetailResultRepository = classForumDetailResultRepository;
            _setTimeRetryClassForumPublisher = setTimeRetryClassForumPublisher;
            _mapper = mapper;
            _appSetting = appSetting;
        }

        public async Task<bool> Handle(SubmitClassforumAICommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var classForumDetailResult = await _classForumDetailResultRepository.GetByIdAsync(request.ClassForumDetailResultId);
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


            #region Retry
            if (aIResponse == null && classForumDetailResult != null && classForumDetailResult.RetryTime <= Max_Time_Retry)
            {
                var model = _mapper.Map<SetTimeRetryClassForumModel>(request);
                await _setTimeRetryClassForumPublisher.Publish(model, cancellationToken);
                classForumDetailResult.RetryTime += 1;
            }
            var classForumDetailResultOwner = _classForumDetailResultRepository.Queryable.Include(x => x.ClassForumResult).FirstOrDefault(x => x.Id == request.ClassForumDetailResultId);

            if (classForumDetailResult != null && classForumDetailResult.RetryTime > Max_Time_Retry)
            {
                await _mediator.Send(new SenderCommand
                {
                    Email = _appSetting!.CustomerSupportConfig!.Email,
                    Content = ValueSettings.CustomerSupport.Content.Format(classForumDetailResultOwner?.CreatedFullName ?? default, classForumDetailResult.ClassForumResult?.LessonResult?.Lesson?.Name ?? default),
                    Subject = ValueSettings.CustomerSupport.TitleMail.Format(classForumDetailResultOwner?.CreatedFullName ?? default)
                }, cancellationToken);
            }

            #endregion

            if (classForumDetailResult != null)
            {
                var classForumAIs = ConvertHelper.Deserialize<List<ClassForumAIModel>>(aIResponse);
                classForumDetailResult.GradingAlFeedback = ConvertHelper.Serialize(classForumAIs);

                _classForumDetailResultRepository.Update(classForumDetailResult);
                await _classForumDetailResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            await _submitAIResponsePublisher.Publish(new SubmitAIResponseModel
            {
                GradingAlFeedback = aIResponse,
                ClassForumResultId = request.ClassForumResultId,
                EnumSubmissionCount = request.SubmissionCount
            }, cancellationToken);

            if (!string.IsNullOrEmpty(aIResponse))
            {

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
