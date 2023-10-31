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


        public DeleteCommentCommandHandler(ICommentRepository commentRepository, AuthContext authContext, NotificationMessagePublisher notificationMessagePublisher, IUserService userService, ITrainingService trainingService, ICourseService courseService)
        {
            _commentRepository = commentRepository;
            _authContext = authContext;
            _notificationMessagePublisher = notificationMessagePublisher;
            _userService = userService;
            _trainingService = trainingService;
            _courseService = courseService;
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

                var (returnedParamsLink, objectOwnerId) = CustomDataForParamMessage(comment.Id, comment!, classForumResultTemp?.CourseId, classForumResultTemp?.UnitId);

                NotificationQueueModel notificationQueueModel = new NotificationQueueModel()
                {
                    UserId = comment.CreatedUserId,
                    Type = EnumNotificationType.LinkComment,
                    Content = EnumNotificationContent.DeleteComment,
                    SenderId = _authContext.CurrentUserId,
                    ParamsLink = returnedParamsLink,
                    ObjectId = comment.Id
                };

                await _notificationMessagePublisher.Publish(notificationQueueModel, cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = result;
                return methodResult;
            });
            return methodResult;
        }

        public static (List<object> paramsLink, Guid ownerObjectId) CustomDataForParamMessage(Guid objectId, dynamic templateResult, Guid? courseId, Guid? unitId)
        {
            if (templateResult == null)
            {
                return (new List<object>(), Guid.Empty);
            }

            // param
            var paramsLink = new List<object> { unitId?.ToString() ?? string.Empty, courseId?.ToString() ?? string.Empty, templateResult?.Id.ToString() ?? string.Empty, objectId };
            var ownerObjectId = templateResult?.CreatedUserId ?? default;

            return (paramsLink, ownerObjectId);
        }
    }
}
