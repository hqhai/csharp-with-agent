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
    using Fsel.Course.Lms.Application.Queries.OtherFeatureQuery;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class UpdateStatusClassForumResultCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }

    public class UpdateStatusClassForumResultCommandHandler : IRequestHandler<UpdateStatusClassForumResultCommand, MethodResult<bool>>
    {
        private readonly IClassForumResultRepository _classForumResulRepository;
        private readonly NotificationMessagePublisher _notificationMessagePublisher;
        private readonly AuthContext _authContext;
        private readonly ICourseRepository _courseRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IMediator _mediator;

        public UpdateStatusClassForumResultCommandHandler(IClassForumResultRepository classForumResulRepository, NotificationMessagePublisher notificationMessagePublisher, AuthContext authContext, ICourseRepository courseRepository, IUnitRepository unitRepository, ILessonResultRepository lessonResultRepository, IMediator mediator)
        {
            _classForumResulRepository = classForumResulRepository;
            _notificationMessagePublisher = notificationMessagePublisher;
            _authContext = authContext;
            _courseRepository = courseRepository;
            _unitRepository = unitRepository;
            _lessonResultRepository = lessonResultRepository;
            _mediator = mediator;
        }

        public async Task<MethodResult<bool>> Handle(UpdateStatusClassForumResultCommand request, CancellationToken cancellationToken)
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

            var lesson = await _lessonResultRepository.GetIncludeByIdAsync(classForumResult.LessonResultId);

            await _classForumResulRepository.ExecuteTransactionAsync((Func<Task<VoidMethodResult>>)(async () =>
             {
                 classForumResult.Status = EnumClassForumResultStatus.Denied;
                 _classForumResulRepository.Update(classForumResult, false, x => x.LessonResultId, x => x.ClassForumId, x => x.StudentId);
                 await _classForumResulRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                 var objectOwnerId = CustomDataForParamMessage(classForumResult!);

                 var query = new GetFeatureModuleQuery
                 {
                     FeatureModule = EnumFeatureModule.ClassForumResult,
                     ObjectId = classForumResult?.Id ?? default
                 };

                 var featureModule = await _mediator.Send(query).ConfigureAwait(false);
                 var featureModuleResult = featureModule?.Result;

                 var paramLinksValue = new List<object> { featureModuleResult?.CourseId ?? default, featureModuleResult?.UnitId ?? default, featureModuleResult?.LessonId ?? default, featureModuleResult?.ClassForumDetailResultId ?? default };

                 var notificationQueueModel = new NotificationSendingQueueModel()
                 {
                     Type = EnumNotificationType.LinkPage,
                     Content = EnumNotificationContent.DeleteClassForumResult,
                     SenderId = _authContext.CurrentUserId,
                     ParamsLink = paramLinksValue,
                     ObjectId = classForumResult?.Id ?? Guid.NewGuid(),
                     PlatformCode = EnumPlatformCode.LMS,
                     ParamsMessage = new List<object> { classForumResult?.CreatedFullName ?? string.Empty },
                     UserIds = new List<Guid>() { objectOwnerId },
                 };

                 await _notificationMessagePublisher.Publish(notificationQueueModel, cancellationToken).ConfigureAwait(false);

                 methodResult.StatusCode = StatusCodes.Status200OK;
                 methodResult.Result = true;
                 return methodResult;
             }));

            return methodResult;
        }

        public static Guid CustomDataForParamMessage(dynamic templateResult)
        {
            if (templateResult == null)
            {
                return Guid.Empty;
            }

            var ownerObjectId = templateResult?.CreatedUserId ?? default;

            return ownerObjectId;
        }
    }
}
