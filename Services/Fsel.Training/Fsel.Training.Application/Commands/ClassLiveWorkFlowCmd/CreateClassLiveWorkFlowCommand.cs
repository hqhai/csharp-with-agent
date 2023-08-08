// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Commands.ClassLiveWorkFlowCmd
{
    using System;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Shared.Enums;
    using Fsel.Training.Application.Services.SystemServices;
    using Fsel.Training.Application.Services.UserServices;
    using Fsel.Training.Domain.Entities;
    using Fsel.Training.Domain.Enums.ErrorCodes;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.CommandModels.ClassLiveWorkFlows;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateClassLiveWorkFlowCommand : CreateClassLiveWorkFlowCommandModel, IRequest<MethodResult<ClassLiveWorkFlowModel>>
    {
    }

    public class CreateClassLiveWorkFlowCommandHandler : IRequestHandler<CreateClassLiveWorkFlowCommand, MethodResult<ClassLiveWorkFlowModel>>
    {
        private readonly IClassLiveWorkFlowRepository _classLiveWorkFlowRepository;
        private readonly IClassLiveCalendarRepository _classLiveCalendarRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly ISystemService _systemService;
        private readonly IMapper _mapper;

        public CreateClassLiveWorkFlowCommandHandler(IClassLiveWorkFlowRepository classLiveWorkFlowRepository,
            IClassLiveCalendarRepository classLiveCalendarRepository,
            AuthContext authContext,
            ISystemService systemService,
            IUserService userService,
            IMapper mapper)
        {
            _classLiveWorkFlowRepository = classLiveWorkFlowRepository;
            _classLiveCalendarRepository = classLiveCalendarRepository;
            _authContext = authContext;
            _systemService = systemService;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<MethodResult<ClassLiveWorkFlowModel>> Handle(CreateClassLiveWorkFlowCommand request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<ClassLiveWorkFlowModel>();
            ArgumentNullException.ThrowIfNull(request);

            var teacherResult = await _userService.GetTeacherByUserIdAsync(_authContext.CurrentUserId);
            if (!teacherResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(teacherResult));
                return methodResult;
            }
            var teacherId = teacherResult.Content?.Result?.Id;

            var classLiveCalendar = await _classLiveCalendarRepository.Queryable.Include(x => x.Class).FirstOrDefaultAsync(x => x.Id == request.ClassLiveCalendarId, cancellationToken);
            if (classLiveCalendar == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(classLiveCalendar));
                return methodResult;
            }
            var classLiveWorkFlow = await _classLiveWorkFlowRepository.Queryable.FirstOrDefaultAsync(x => x.TeacherId == teacherId && x.ClassLiveCalendarId == request.ClassLiveCalendarId && x.Status == EnumWorkFlowAssignTeacherStatus.Pending.ToString(), cancellationToken);
            if (classLiveWorkFlow != null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(classLiveWorkFlow));
                return methodResult;
            }
            else
            {
                var listLiveTimeFrameResults = await _systemService.GetLiveTimeFramesAsync();
                var liveTimeFrames = listLiveTimeFrameResults.Content?.Result;

                var liveTimeFrameCalendar = liveTimeFrames?.FirstOrDefault(x => x.Id == classLiveCalendar.LiveTimeFrameId);
                var startTime = liveTimeFrameCalendar?.StartTime;
                var endTime = liveTimeFrameCalendar?.EndTime;

                var liveDate = classLiveCalendar.LiveDate.Date.AddHours(startTime ?? 0);
                var startTimeLive = liveDate.AddHours(-24);
                classLiveWorkFlow = new ClassLiveWorkFlow
                {
                    ClassLiveCalendarId = request.ClassLiveCalendarId,
                    Type = request.Type,
                    CsoId = classLiveCalendar.Class?.CsoId,
                    Description = request.Description,
                    TeacherId = teacherId
                };
                var status = string.Empty;
                if (request.Type == EnumWorkFlowType.ChangeTeacher)
                {
                    status = EnumWorkFlowChangeTeacherStatus.RequestChangeTeacher.ToString();
                    DateTime dateTime = DateTime.Now;
                    if (startTimeLive < dateTime)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumClassLiveWorkFlowErrorCode.CanNotChangeTeacher));
                        return methodResult;
                    }
                }
                else if (request.Type == EnumWorkFlowType.CancelSchedule && request.ClassLiveWorkFlowPlans != null)
                {
                    status = EnumWorkFlowCancelScheduleStatus.RequestCancel.ToString();

                    DateTime dateTime = DateTime.Now;
                    var learnAgainDate = liveDate.AddHours(startTime ?? 0).AddDays(2);
                    var endTimeLive = liveDate.AddHours(startTime ?? 0).AddHours(-1);
                    var check = dateTime > startTimeLive && dateTime < endTimeLive;
                    if (!check)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumClassLiveWorkFlowErrorCode.CanNotCancelLiveTime));
                        return methodResult;
                    }
                    var classLiveCalendars = await _classLiveCalendarRepository.Queryable.Where(x => x.TeacherId == teacherId).ToListAsync(cancellationToken);

                    foreach (var workFlowPlan in request.ClassLiveWorkFlowPlans)
                    {
                        if (classLiveCalendars.Any(x => x.LiveTimeFrameId == workFlowPlan.LiveTimeFrameId && x.LiveDate == workFlowPlan.LiveDate))
                        {
                            methodResult.AddErrorBadRequest(nameof(EnumClassLiveWorkFlowErrorCode.CanNotCancelLiveTime));
                            return methodResult;
                        }
                        var listLiveTimeFramePlan = liveTimeFrames?.FirstOrDefault(x => x.Id == workFlowPlan.LiveTimeFrameId);
                        if (listLiveTimeFramePlan != null && liveTimeFrameCalendar != null)
                        {
                            var livePlan = workFlowPlan.LiveDate.Date.AddHours(listLiveTimeFramePlan.StartTime ?? 0);
                            switch (true)
                            {
                                case var solutionTwo when solutionTwo == liveDate.AddDays(2) <= livePlan:
                                    break;

                                default:
                                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(listLiveTimeFramePlan));
                                    return methodResult;
                            }
                        }
                    }
                    classLiveWorkFlow.ClassLiveWorkFlowPlans = _mapper.Map<IList<ClassLiveWorkFlowPlan>>(request.ClassLiveWorkFlowPlans);
                    classLiveWorkFlow.ClassLiveWorkFlowPlans.ForEach(x => x.IsActive = true);
                }
                classLiveWorkFlow.Status = status;
            }
            await _classLiveWorkFlowRepository.ExecuteTransactionAsync(async () =>
            {
                classLiveWorkFlow = _classLiveWorkFlowRepository.Add(classLiveWorkFlow);
                await _classLiveWorkFlowRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<ClassLiveWorkFlowModel>(classLiveWorkFlow);
                return methodResult;
            });
            return methodResult;
        }
    }
}
