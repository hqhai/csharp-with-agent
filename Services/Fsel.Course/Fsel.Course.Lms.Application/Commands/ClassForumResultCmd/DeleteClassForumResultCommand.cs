// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.ClassForumResultCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class DeleteClassForumResultCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }

    public class DeleteClassForumResultCommandHandler : IRequestHandler<DeleteClassForumResultCommand, MethodResult<bool>>
    {
        private readonly IClassForumResultRepository _classForumResulRepository;
        private readonly NotificationMessagePublisher _notificationMessagePublisher;
        private readonly AuthContext _authContext;
        private readonly ICourseRepository _courseRepository;
        private readonly IUnitRepository _unitRepository;

        public DeleteClassForumResultCommandHandler(IClassForumResultRepository classForumResulRepository, NotificationMessagePublisher notificationMessagePublisher, AuthContext authContext, ICourseRepository courseRepository, IUnitRepository unitRepository)
        {
            _classForumResulRepository = classForumResulRepository;
            _notificationMessagePublisher = notificationMessagePublisher;
            _authContext = authContext;
            _courseRepository = courseRepository;
            _unitRepository = unitRepository;
        }

        public async Task<MethodResult<bool>> Handle(DeleteClassForumResultCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var classForumResult = await _classForumResulRepository.GetIncludeByIdAsync(request.Id);

            if (classForumResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(classForumResult));
                return methodResult;
            }
            if (classForumResult.Status != EnumClassForumResultStatus.PendingForGrading && classForumResult.Status != EnumClassForumResultStatus.Graded)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassForumResultErrorCode.CanNotDeleteInCurrentStatus), nameof(classForumResult.Status), classForumResult.Status);
                return methodResult;
            }
            await _classForumResulRepository.ExecuteTransactionAsync(async () =>
        {
            var result = await _classForumResulRepository.DeleteAsync(classForumResult);
            await _classForumResulRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

            var (returnedParamsLink, objectOwnerId) = CustomDataForParamMessage(classForumResult?.Id, classForumResult!, classForumResult?.LessonResult?.CourseId, classForumResult?.LessonResult?.UnitId);

            NotificationQueueModel notificationQueueModel = new NotificationQueueModel()
            {
                UserId = classForumResult?.CreatedUserId,
                Type = EnumNotificationType.LinkPage,
                Content = EnumNotificationContent.DeleteClassForumResult,
                SenderId = _authContext.CurrentUserId,
                ParamsLink = returnedParamsLink,
                ObjectId = classForumResult?.Id ?? Guid.NewGuid(),
                PlatformCode = EnumPlatformCode.LMS
            };

            await _notificationMessagePublisher.Publish(notificationQueueModel, cancellationToken).ConfigureAwait(false);

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = result;
            return methodResult;
        });

            return methodResult;
        }

        public static (List<object> paramsLink, Guid ownerObjectId) CustomDataForParamMessage(Guid? objectId, dynamic templateResult, Guid? courseId, Guid? unitId)
        {
            if (templateResult == null)
            {
                return (new List<object>(), Guid.Empty);
            }

            if (objectId == null)
            {
                return (new List<object>(), Guid.Empty);

            }

            // param
            var paramsLink = new List<object> { unitId.ToString() ?? string.Empty, courseId.ToString() ?? string.Empty, templateResult?.Id.ToString() ?? string.Empty, objectId };
            var ownerObjectId = templateResult?.CreatedUserId ?? default;


            return (paramsLink, ownerObjectId);
        }
    }
}
