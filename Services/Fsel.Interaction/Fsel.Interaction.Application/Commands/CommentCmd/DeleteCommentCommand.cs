// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Commands.CommentCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Interaction.Application.Queues.Publishers;
    using Fsel.Interaction.Application.Services.CourseServices;
    using Fsel.Interaction.Application.Services.CourseServices.Models;
    using Fsel.Interaction.Application.Services.CourseServices.QueryModel;
    using Fsel.Interaction.Application.Services.TrainingServices;
    using Fsel.Interaction.Application.Services.UserServices;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Shared.Enums;

    public class DeleteCommentCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }

    public class DeleteCommentCommandHandler : IRequestHandler<DeleteCommentCommand, MethodResult<bool>>
    {
        private readonly ICommentRepository _commentRepository;
        private readonly AuthContext _authContext;
        private readonly NotificationMessagePublisher _notificationMessagePublisher;
        private readonly IUserService _userService;
        private readonly ITrainingService _trainingService;
        private readonly ICourseService _courseService;
        private readonly IMediator _mediator;


        public DeleteCommentCommandHandler(ICommentRepository commentRepository, AuthContext authContext, NotificationMessagePublisher notificationMessagePublisher, IUserService userService, ITrainingService trainingService, ICourseService courseService, IMediator mediator)
        {
            _commentRepository = commentRepository;
            _authContext = authContext;
            _notificationMessagePublisher = notificationMessagePublisher;
            _userService = userService;
            _trainingService = trainingService;
            _courseService = courseService;
            _mediator = mediator;
        }

        public async Task<MethodResult<bool>> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var comment = await _commentRepository.GetByIdAsync(request.Id);
            if (comment == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(comment));
                return methodResult;
            }

            await _commentRepository.ExecuteTransactionAsync(async () =>
            {
                var result = await _commentRepository.DeleteAsync(comment);
                await _commentRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                var classForumResult = await _courseService.GetClassForumResultByIdAsync(comment.ObjectId);
                var classForumResultTemp = classForumResult?.Content?.Result;

                var lesson = await _courseService.GetLessonResult(classForumResultTemp.LessonResultId ?? default);

                var objectOwnerId = CustomDataForParamMessage(comment!);

                FeatureModuleQuery query = new FeatureModuleQuery
                {
                    FeatureModule = EnumFeatureModule.ClassForumDetailResult,
                    ObjectId = classForumResultTemp?.Id ?? default
                };

                var featureModule = await _courseService.GetModuleModel(query);
                var featureModuleResult = featureModule?.Content?.Result;

                List<object> paramLinksValue = new List<object> { featureModuleResult?.CourseId ?? default, featureModuleResult?.UnitId ?? default, featureModuleResult?.LessonId ?? default, featureModuleResult?.ClassForumDetailResultId ?? default };

                List<object> paramMessages = new List<object> { classForumResultTemp?.CreatedFullName?.ToString() ?? string.Empty };

                NotificationSendingQueueModel notificationQueueModel = new NotificationSendingQueueModel()
                {
                    UserIds = new List<Guid>() { comment.CreatedUserId },
                    Type = EnumNotificationType.LinkComment,
                    Content = EnumNotificationContent.DeleteComment,
                    SenderId = _authContext.CurrentUserId,
                    ParamsLink = paramLinksValue,
                    ParamsMessage = paramMessages,
                    ObjectId = comment.Id,
                    PlatformCode = EnumPlatformCode.LMS
                };

                await _notificationMessagePublisher.Publish(notificationQueueModel, cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = result;
                return methodResult;
            });
            return methodResult;
        }

        public static Guid CustomDataForParamMessage(dynamic templateResult)
        {
            if (templateResult == null)
            {
                return Guid.Empty;
            }

            // param
            var ownerObjectId = templateResult?.CreatedUserId ?? default;

            return ownerObjectId;
        }
    }
}
