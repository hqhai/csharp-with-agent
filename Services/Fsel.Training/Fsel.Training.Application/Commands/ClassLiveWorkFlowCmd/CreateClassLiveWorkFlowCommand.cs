// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Commands.ClassLiveWorkFlowCmd
{
    using System;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
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
                methodResult.AddErrorBadRequest(nameof(EnumClassLiveWorkFlowErrorCode.TeacherNotExits));
                return methodResult;
            }
            var teacherId = teacherResult.Content?.Result?.Id;

            var classLiveCalendar = await _classLiveCalendarRepository.GetByIdAsync(request.ClassLiveCalendarId);
            if (classLiveCalendar == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassLiveCalendarErrorCode.ClassLiveCalendarNotExits), nameof(request.ClassLiveCalendarId), request.ClassLiveCalendarId);
                return methodResult;
            }
            var classLiveWorkFlow = await _classLiveWorkFlowRepository.Queryable.FirstOrDefaultAsync(x => x.TeacherId == teacherId && x.ClassLiveCalendarId == request.ClassLiveCalendarId && x.Status == EnumWorkFlowAssignTeacherStatus.Pending.ToString(), cancellationToken);
            if (classLiveWorkFlow != null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassLiveWorkFlowErrorCode.ClassLiveWorkFlowAlreadyExist));
                return methodResult;
            }
            else
            {
                classLiveWorkFlow = new ClassLiveWorkFlow
                {
                    ClassLiveCalendarId = request.ClassLiveCalendarId,
                    Type = request.Type,
                    Description = request.Description,
                    TeacherId = teacherId
                };
                var status = string.Empty;
                if (request.Type == EnumWorkFlowType.ChangeTeacher)
                {
                    status = EnumWorkFlowChangeTeacherStatus.RequestChangeTeacher.ToString();
                    DateTime dateTime = DateTime.Now;
                    var assignTeacher = classLiveCalendar.LiveDate.AddDays(-1);
                    if (assignTeacher.Date < dateTime.Date)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumClassLiveWorkFlowErrorCode.CanNotChangeTeacher));
                        return methodResult;
                    }
                }
                else if (request.Type == EnumWorkFlowType.CancelSchedule && request.ClassLiveWorkFlowPlans != null)
                {
                    status = EnumWorkFlowCancelScheduleStatus.RequestCancel.ToString();
                    DateTime dateTime = DateTime.Now;
                    var learnAgainDate = classLiveCalendar.LiveDate.AddDays(2);
                    var endTime = classLiveCalendar.LiveDate.AddHours(-1);
                    var startTime = classLiveCalendar.LiveDate.AddHours(-24);
                    var check = dateTime > startTime && dateTime < endTime;

                    if (!check)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumClassLiveWorkFlowErrorCode.CanNotCancelLiveTime));
                        return methodResult;
                    }

                    var listLiveTimeFrameResults = await _systemService.GetLiveTimeFramesAsync();
                    var classLiveCalendars = await _classLiveCalendarRepository.Queryable.Where(x => x.TeacherId == teacherId).ToListAsync(cancellationToken);

                    var liveTimeFrames = listLiveTimeFrameResults.Content?.Result;
                    foreach (var workFlowPlan in request.ClassLiveWorkFlowPlans)
                    {
                        if (classLiveCalendars.Any(x => x.LiveTimeFrameId == workFlowPlan.LiveTimeFrameId && x.LiveDate == workFlowPlan.LiveDate))
                        {
                            methodResult.AddErrorBadRequest(nameof(EnumClassLiveWorkFlowErrorCode.CanNotCancelLiveTime));
                            return methodResult;
                        }
                        var listLiveTimeFrameCalendar = liveTimeFrames?.FirstOrDefault(x => x.Id == classLiveCalendar.LiveTimeFrameId);
                        var listLiveTimeFramePlan = liveTimeFrames?.FirstOrDefault(x => x.Id == workFlowPlan.LiveTimeFrameId);
                        if (listLiveTimeFramePlan != null && listLiveTimeFrameCalendar != null)
                        {
                            switch (true)
                            {
                                case var solutionOne when solutionOne == (listLiveTimeFramePlan.StartTime <= listLiveTimeFrameCalendar.StartTime && classLiveCalendar.LiveDate.AddDays(3) <= workFlowPlan.LiveDate):
                                    break;

                                case var solutionTwo when solutionTwo == (listLiveTimeFramePlan.StartTime <= listLiveTimeFrameCalendar.StartTime && classLiveCalendar.LiveDate.AddDays(2) <= workFlowPlan.LiveDate):
                                    break;

                                default:
                                    methodResult.AddErrorBadRequest(nameof(EnumClassLiveWorkFlowErrorCode.LiveDateInvalid));
                                    return methodResult;
                            }
                        }
                    }
                    classLiveWorkFlow.ClassLiveWorkFlowPlans = _mapper.Map<IList<ClassLiveWorkFlowPlan>>(request.ClassLiveWorkFlowPlans);
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
