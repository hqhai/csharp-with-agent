// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Commands.ClassLiveCmd
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Shared.Enums;
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
        private readonly IClassLiveCalendarRepository _classLiveCalendarRepository;
        private readonly IClassLiveWorkFlowRepository _classLiveWorkFlowRepository;
        private readonly IClassRepository _classRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;

        public ApproveClassLiveCommandHandler(
            IClassLiveCalendarRepository classLiveCalendarRepository,
            IClassLiveWorkFlowRepository classLiveWorkFlowRepository,
            IClassRepository classRepository,
            AuthContext authContext,
            IUserService userService)
        {
            _classLiveCalendarRepository = classLiveCalendarRepository;
            _classLiveWorkFlowRepository = classLiveWorkFlowRepository;
            _classRepository = classRepository;
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
                methodResult.AddErrorBadRequest(nameof(EnumClassLiveWorkFlowErrorCode.TeacherNotExits));
                return methodResult;
            }
            var teacherId = teacherResult.Content?.Result?.Id;
            var @class = await _classRepository.Queryable.FirstOrDefaultAsync(x => x.TeacherId == teacherId && x.Id == request.Id, cancellationToken);
            var classLiveCalenda1 = await _classLiveCalendarRepository.Queryable.Include(x => x.ClassLiveWorkFlows)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            var classLiveCalendar = await _classLiveCalendarRepository.Queryable.Include(x => x.ClassLiveWorkFlows)
                .FirstOrDefaultAsync(x => x.Id == request.Id && x.ClassLiveWorkFlows.Any(x => x.TeacherId == teacherId), cancellationToken);
            if (classLiveCalendar != null || @class != null)
            {
                if (classLiveCalendar != null)
                {
                    var classLiveWorkFlow = classLiveCalendar.ClassLiveWorkFlows.FirstOrDefault(x => x.Status == EnumWorkFlowAssignTeacherStatus.Pending.ToString() && x.TeacherId == teacherId);
                    var classLiveWorkFlowParent = await _classLiveWorkFlowRepository.GetByIdAsync(classLiveWorkFlow?.WorkFlowParentId ?? default);
                    if (classLiveWorkFlow != null && classLiveWorkFlow.Type == EnumWorkFlowType.AssignTeacher && classLiveWorkFlowParent != null)
                    {
                        if (request.IsAcept)
                        {
                            classLiveWorkFlowParent.Status = EnumWorkFlowChangeTeacherStatus.DoneScheduled.ToString();

                            classLiveCalendar.TeacherId = teacherId;

                            classLiveWorkFlow.Status = EnumWorkFlowAssignTeacherStatus.Approved.ToString();
                            classLiveWorkFlow.Description = request.Description;
                            _classLiveCalendarRepository.Update(classLiveCalendar);
                            await _classLiveCalendarRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
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
                    if (@class.TeacherApprovalStatus == EnumTeacherApprovalStatus.Approved)
                    {
                        if (request.IsAcept)
                        {
                            @class.TeacherApprovalStatus = EnumTeacherApprovalStatus.Pending;
                        }
                        else
                        {
                            @class.TeacherApprovalStatus = default;
                            @class.TeacherId = default;
                        }
                        if (!@class.IsValid())
                        {
                            methodResult.AddErrorBadRequest(@class.ErrorMessages);
                            return methodResult;
                        }
                        _classRepository.Update(@class);
                        await _classRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumClassLiveWorkFlowErrorCode.ClassNotStatusApproved));
                        return methodResult;
                    }
                }
            }
            else
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassLiveCalendarErrorCode.ClassLiveCalendarNotExits));
                return methodResult;
            }

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = true;
            return methodResult;
        }
    }
}
