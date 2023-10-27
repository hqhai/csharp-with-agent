// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Commands.ClassLiveCmd
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Training.Application.Services.SystemServices;
    using Fsel.Training.Application.Services.UserServices;
    using Fsel.Training.Domain.Entities;
    using Fsel.Training.Domain.Enums.ErrorCodes;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.CommandModels.ClassLives;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class ApproveClassLiveCommand : ApproveClassLiveCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class ApproveClassLiveCommandHandler : IRequestHandler<ApproveClassLiveCommand, MethodResult<bool>>
    {
        private readonly IClassLiveWorkFlowRepository _classLiveWorkFlowRepository;
        private readonly ISystemService _systemService;
        private readonly IClassLiveCalendarRepository _classLiveCalendarRepository;
        private readonly IClassRepository _classRepository;
        private readonly ITeacherFreeDateRepository _teacherFreeDateRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;

        public ApproveClassLiveCommandHandler(
            IClassLiveWorkFlowRepository classLiveWorkFlowRepository,
            ISystemService systemService,
            IClassLiveCalendarRepository classLiveCalendarRepository,
            IClassRepository classRepository,
            ITeacherFreeDateRepository teacherFreeDateRepository,
            AuthContext authContext,
            IUserService userService)
        {
            _classLiveWorkFlowRepository = classLiveWorkFlowRepository;
            _systemService = systemService;
            _classLiveCalendarRepository = classLiveCalendarRepository;
            _classRepository = classRepository;
            _teacherFreeDateRepository = teacherFreeDateRepository;
            _authContext = authContext;
            _userService = userService;
        }

        public async Task<MethodResult<bool>> Handle(ApproveClassLiveCommand request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<bool>();
            ArgumentNullException.ThrowIfNull(request);

            var teacherResult = await _userService.GetTeacherByUserIdAsync(_authContext.CurrentUserId);
            if (!teacherResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(teacherResult));
                return methodResult;
            }
            var teacherId = teacherResult.Content?.Result?.Id;
            var @class = await _classRepository.Queryable.FirstOrDefaultAsync(x => x.TeacherId == teacherId && x.Id == request.Id, cancellationToken);
            var classLiveWorkFlow = await _classLiveWorkFlowRepository.Queryable.Include(x => x.ClassLiveCalendar)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (classLiveWorkFlow != null || @class != null)
            {
                if (classLiveWorkFlow != null)
                {
                    var classLiveWorkFlowParent = await _classLiveWorkFlowRepository.GetByIdAsync(classLiveWorkFlow.WorkFlowParentId ?? default);
                    if (classLiveWorkFlow.Type == EnumWorkFlowType.AssignTeacher && classLiveWorkFlowParent != null && classLiveWorkFlow.ClassLiveCalendar != null)
                    {
                        if (request.IsAccept)
                        {
                            classLiveWorkFlowParent.Status = EnumWorkFlowChangeTeacherStatus.DoneScheduled.ToString();
                            classLiveWorkFlow.ClassLiveCalendar.TeacherId = teacherId;
                            classLiveWorkFlow.Status = EnumWorkFlowAssignTeacherStatus.Approved.ToString();
                            classLiveWorkFlow.Description = request.Description;
                        }
                        else
                        {
                            classLiveWorkFlow.Status = EnumWorkFlowAssignTeacherStatus.Reject.ToString();
                            classLiveWorkFlow.Description = request.Description;
                        }
                        if (!classLiveWorkFlow.IsValid())
                        {
                            methodResult.AddErrorBadRequest(classLiveWorkFlow.ErrorMessages);
                            return methodResult;
                        }
                        _classLiveWorkFlowRepository.UpdateList(new List<ClassLiveWorkFlow> { classLiveWorkFlow, classLiveWorkFlowParent });
                        await _classLiveWorkFlowRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumClassLiveWorkFlowErrorCode.ClassLiveWorkFlowStatusAssignTeacher));
                        return methodResult;
                    }
                }
                else if (@class != null)
                {
                    if (@class.TeacherApprovalStatus == EnumTeacherApprovalStatus.Pending)
                    {
                        List<ClassLiveCalendar>? classLiveCalendars = default;
                        TeacherFreeDate? teacherFreeDate = default;
                        if (request.IsAccept)
                        {
                            @class.TeacherApprovalStatus = EnumTeacherApprovalStatus.Approved;
                            var liveTimeFrameResults = await _systemService.GetLiveTimeFramesAsync();
                            if (!liveTimeFrameResults.IsSuccessStatusCode)
                            {
                                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallSystemServiceError));
                                return methodResult;
                            }
                            var liveTimeFrames = liveTimeFrameResults?.Content?.Result;
                            var classLives = await _classLiveCalendarRepository.Queryable.Where(x => x.ClassId == @class.Id).ToListAsync(cancellationToken);
                            if (classLives != null && classLives.Count > 0)
                            {
                                classLiveCalendars = classLives.Where(classLive => liveTimeFrames?.FirstOrDefault(x => x.Id == classLive.LiveTimeFrameId)?.StartTime != null &&
                                           classLive.LiveDate.Date.AddHours(liveTimeFrames.First(x => x.Id == classLive.LiveTimeFrameId)?.StartTime ?? default) > DateTime.UtcNow
                                       ).Select(x =>
                                       {
                                           x.TeacherId = @class.TeacherId;
                                           return x;
                                       })
                                   .ToList();
                            }
                        }
                        else
                        {
                            teacherFreeDate = await _teacherFreeDateRepository.Queryable
                                   .Include(x => x.TeacherFreeTimes)
                                   .Where(x => @class.StartDate.HasValue && @class.EndDate.HasValue && x.StartDate.Date <= @class.StartDate.Value.Date && x.EndDate.Date >= @class.EndDate.Value.Date && x.TeacherId == teacherId)
                                   .FirstOrDefaultAsync(cancellationToken);
                            if (teacherFreeDate == null)
                            {
                                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(teacherFreeDate));
                                return methodResult;
                            }
                            teacherFreeDate.TeacherFreeTimes = teacherFreeDate.TeacherFreeTimes.Where(x => !(@class.LiveDays != null && @class.LiveDays.Any() && @class.LiveDays.All(n => x.DayOfWeek == n) && x.LiveTimeFrameId == @class.LiveTimeFrameId)).ToList();

                            @class.TeacherApprovalStatus = default;
                            @class.TeacherId = default;
                        }
                        if (!@class.IsValid())
                        {
                            methodResult.AddErrorBadRequest(@class.ErrorMessages);
                            return methodResult;
                        }
                        if (classLiveCalendars != null && classLiveCalendars.Any())
                        {
                            _classLiveCalendarRepository.UpdateList(classLiveCalendars);
                            await _classLiveCalendarRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                        }
                        if (teacherFreeDate != null)
                        {
                            _teacherFreeDateRepository.Update(teacherFreeDate);
                            await _teacherFreeDateRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                        }

                        _classRepository.Update(@class);
                        await _classRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumClassLiveWorkFlowErrorCode.ClassNotStatusPedding));
                        return methodResult;
                    }
                }
            }
            else
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(classLiveWorkFlow));
                return methodResult;
            }

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = true;
            return methodResult;
        }
    }
}