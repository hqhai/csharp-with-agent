// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Commands.CancelScheduleLiveCmd
{
    using System.Globalization;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.Training.Application.Services.UserServices;
    using Fsel.Training.Domain.Entities;
    using Fsel.Training.Domain.Enums.ErrorCodes;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.CommandModels.CancelScheduleLives;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Fsel.Training.Application.Queues.Publishers;
    public class ReferenceCalendarCommand : CancelScheduleLiveCommandModel, IRequest<MethodResult<ClassLiveWorkFlowModel>>
    {
    }

    public class ReferenceCalendarCommandHandler : IRequestHandler<ReferenceCalendarCommand, MethodResult<ClassLiveWorkFlowModel>>
    {
        private readonly IClassLiveWorkFlowRepository _classLiveWorkFlowRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly NotificationMessagePublisher _notificationMessagePublisher;

        public ReferenceCalendarCommandHandler(IClassLiveWorkFlowRepository classLiveWorkFlowRepository
            , AuthContext authContext
            , IUserService userService
            , IMapper mapper
            , NotificationMessagePublisher notificationMessagePublisher)
        {
            _classLiveWorkFlowRepository = classLiveWorkFlowRepository;
            _authContext = authContext;
            _userService = userService;
            _mapper = mapper;
            _notificationMessagePublisher = notificationMessagePublisher;
        }

        public async Task<MethodResult<ClassLiveWorkFlowModel>> Handle(ReferenceCalendarCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ClassLiveWorkFlowModel> methodResult = new MethodResult<ClassLiveWorkFlowModel>();

            if (request.ClassLiveWordFlowPlans == null || request.ClassLiveWordFlowPlans.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassLiveWorkFlowErrorCode.ClassLiveWorkFlowPlansNull));
                return methodResult;
            }

            var classLiveWorkFlow = await _classLiveWorkFlowRepository.Queryable
                                                .Include(x => x.ClassLiveWorkFlowPlans)
                                                .Include(x => x.ClassLiveCalendar)
                                                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (classLiveWorkFlow == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(classLiveWorkFlow));
                return methodResult;
            }
            var csoResult = await _userService.GetCsoByUserIdAsync(_authContext.CurrentUserId);
            if (!csoResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(csoResult));
                return methodResult;
            }
            var cso = csoResult.Content?.Result;
            foreach (var item in request.ClassLiveWordFlowPlans)
            {
                var classWordFlowPlan = classLiveWorkFlow.ClassLiveWorkFlowPlans.FirstOrDefault(x => x.Id == item.ClassLiveWordFlowPlanId);
                if (classWordFlowPlan != null)
                {
                    classWordFlowPlan.IsActive = item.IsActive;
                }
            }
            if (classLiveWorkFlow.Status == EnumWorkFlowCancelScheduleStatus.RequestCancel.ToString())
            {
                classLiveWorkFlow.CsoId = cso?.Id;
                classLiveWorkFlow.Status = EnumWorkFlowCancelScheduleStatus.WaitVote.ToString();
            }
            else if (classLiveWorkFlow.Status == EnumWorkFlowCancelScheduleStatus.WaitVote.ToString() && classLiveWorkFlow.ClassLiveCalendar != null)
            {
                if (classLiveWorkFlow.UpdatedDate != null && classLiveWorkFlow.UpdatedDate.Value.AddDays(2) > DateTime.UtcNow)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumClassLiveWorkFlowErrorCode.VotingTimeIsNotEnoughForTwoDays));
                    return methodResult;
                }
                classLiveWorkFlow.CsoId = cso?.Id;
                classLiveWorkFlow.Status = EnumWorkFlowCancelScheduleStatus.DoneScheduled.ToString();
                var classLiveCalendar = request.ClassLiveWordFlowPlans.FirstOrDefault(x => x.IsActive);
                classLiveWorkFlow.ClassLiveCalendar.LiveDate = classLiveCalendar!.LiveDate;
                classLiveWorkFlow.ClassLiveCalendar.LiveTimeFrameId = classLiveCalendar.LiveTimeFrameId;
            }

            await _classLiveWorkFlowRepository.ExecuteTransactionAsync(async () =>
            {
                _classLiveWorkFlowRepository.Update(classLiveWorkFlow);
                await _classLiveWorkFlowRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<ClassLiveWorkFlowModel>(classLiveWorkFlow);
                return methodResult;
            });
            return methodResult;
        }

        public async Task SendNotification(ClassLiveCalendar classLiveCalendar, CancellationToken cancellationToken)
        {
            var studentsResult = await _userService.GetStudentByClassIdAsync(classLiveCalendar?.ClassId ?? new Guid());

            var students = studentsResult?.Content?.Result?.Select(x => x.Id).ToList();

            string liveDate = classLiveCalendar?.LiveDate != null ? classLiveCalendar.LiveDate!.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)! : string.Empty;

            NotificationSendingQueueModel notificationQueue = new NotificationSendingQueueModel()
            {
                ObjectId = classLiveCalendar!.Id,
                UserIds = students,
                Type = EnumNotificationType.LinkPopup,
                Content = EnumNotificationContent.ChangeClassLiveTeacher,
                PlatformCode = EnumPlatformCode.LMS,
                ParamsMessage = new List<object> { classLiveCalendar?.Class?.Code ?? string.Empty, liveDate ?? string.Empty },
            };
            await _notificationMessagePublisher.Publish(notificationQueue, cancellationToken);

        }
    }
}
