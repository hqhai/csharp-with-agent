// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.ClassForumResultCmd
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.ClassForumResults;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Queries.OtherFeatureQuery;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class ApproveClassForumPenddingCommand : ApproveClassForumPenddingCommandModel, IRequest<MethodResult<ClassForumResultModel>>
    {
    }

    public class ApproveClassForumPenddingCommandHandler : IRequestHandler<ApproveClassForumPenddingCommand, MethodResult<ClassForumResultModel>>
    {
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly IMapper _mapper;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly NotificationMessagePublisher _notificationMessagePublisher;
        private readonly IMediator _mediator;

        public ApproveClassForumPenddingCommandHandler(IClassForumResultRepository classForumResultRepository, IMapper mapper, AuthContext authContext, IUserService userService, ILessonResultRepository lessonResultRepository, NotificationMessagePublisher notificationMessagePublisher, IMediator mediator)
        {
            _classForumResultRepository = classForumResultRepository;
            _mapper = mapper;
            _authContext = authContext;
            _userService = userService;
            _lessonResultRepository = lessonResultRepository;
            _notificationMessagePublisher = notificationMessagePublisher;
            _mediator = mediator;
        }

        public async Task<MethodResult<ClassForumResultModel>> Handle(ApproveClassForumPenddingCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ClassForumResultModel>();

            var classForumResult = await _classForumResultRepository.Queryable.Include(x => x.ClassForumResultFiles)
                                                                    .Include(x => x.ClassForum)
                                                                    .Include(x => x.ClassForumScores)
                                                                    .Where(e => e.Id == request.ClassForumResultId)
                                                                    .FirstOrDefaultAsync(cancellationToken: cancellationToken);

            if (classForumResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(classForumResult));
                return methodResult;
            }
            var csoResults = await _userService.GetCSOByUserId(_authContext.CurrentUserId);
            var csoId = csoResults.Content?.Result?.Id;

            if (classForumResult.Status != EnumClassForumResultStatus.Pending)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassForumResultErrorCode.ClassForumResultStatusNotPendding));
                return methodResult;
            }
            //if (classForumResult.CheckCsoId != csoId)
            //{
            //    methodResult.AddErrorBadRequest(nameof(EnumClassForumResultErrorCode.CsoInvalid), nameof(classForumResult.CheckCsoId));
            //    return methodResult;
            //}
            await _classForumResultRepository.ExecuteTransactionAsync(async () =>
            {
                Dictionary<EnumNotificationType, EnumNotificationContent> enumNotification = new();

                if (request.IsApprove)
                {
                    classForumResult.Status = EnumClassForumResultStatus.Graded;
                    classForumResult.CheckCsoId = csoId;
                    enumNotification = new Dictionary<EnumNotificationType, EnumNotificationContent>()
                    {
                        { EnumNotificationType.LinkPage, EnumNotificationContent.ApprovePostClassForum }
                    };
                }
                else
                {
                    classForumResult.Status = EnumClassForumResultStatus.Denied;
                    classForumResult.CheckCsoId = csoId;
                    classForumResult.CheckStartDate = null;
                    enumNotification = new Dictionary<EnumNotificationType, EnumNotificationContent>()
                    {
                        { EnumNotificationType.LinkPage, EnumNotificationContent.RejectApprovalPostClassForum }
                    };
                }

                await _classForumResultRepository.BulkUpdateList(new List<ClassForumResult> { classForumResult }, bulk =>
                {
                    bulk.IgnoreOnUpdateExpression = c => new { c.StudentId, c.LessonResultId, c.ClassForumId };
                });

                //Thông báo cho user khi bài viết được phê duyệt
                //await SendNotification(classForumResult, enumNotification, cancellationToken);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<ClassForumResultModel>(classForumResult);
                return methodResult;
            });

            return methodResult;
        }

        public async Task SendNotification(ClassForumResult classForumResult, Dictionary<EnumNotificationType, EnumNotificationContent> enumNotification, CancellationToken cancellationToken)
        {
            if (classForumResult == null)
            {
                return;
            }
            var lessonResult = await _lessonResultRepository.GetIncludeByIdAsync(classForumResult.LessonResultId);
            if (lessonResult != null)
            {
                var user = await _userService.GetUserByStudentId(lessonResult.StudentId);
                var userId = user?.Content?.Result?.Human?.UserId;

                GetFeatureModuleQuery query = new GetFeatureModuleQuery
                {
                    FeatureModule = EnumFeatureModule.ClassForumResult,
                    ObjectId = classForumResult.Id,
                    UserId = userId ?? default
                };

                var featureModule = await _mediator.Send(query, cancellationToken).ConfigureAwait(false);
                var featureModuleResult = featureModule?.Result;

                List<object> paramLinksValue = new List<object> { featureModuleResult?.CourseId.ToString() ?? string.Empty, featureModuleResult?.UnitId.ToString() ?? string.Empty, featureModuleResult?.LessonId.ToString() ?? string.Empty, featureModuleResult?.ClassForumDetailResultId.ToString() ?? string.Empty };

                NotificationSendingQueueModel model = new NotificationSendingQueueModel()
                {
                    ObjectId = lessonResult.StudentId,
                    UserIds = new List<Guid>() { userId ?? default },
                    SenderId = _authContext.CurrentUserId,
                    ParamsLink = paramLinksValue,
                    ParamsMessage = new List<object> { classForumResult.ClassForum?.PromptName ?? string.Empty, },
                    Type = enumNotification.FirstOrDefault().Key,
                    Content = enumNotification.FirstOrDefault().Value
                };

                await _notificationMessagePublisher.Publish(model, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
