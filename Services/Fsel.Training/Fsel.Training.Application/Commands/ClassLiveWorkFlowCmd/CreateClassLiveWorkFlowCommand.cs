// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Commands.ClassLiveWorkFlowCmd
{
    using System;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Training.Application.Services.UserServices;
    using Fsel.Training.Domain.Entities;
    using Fsel.Training.Domain.Enums;
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
        private readonly IMapper _mapper;

        public CreateClassLiveWorkFlowCommandHandler(IClassLiveWorkFlowRepository classLiveWorkFlowRepository,
            IClassLiveCalendarRepository classLiveCalendarRepository,
            AuthContext authContext,
            IUserService userService,
            IMapper mapper)
        {
            _classLiveWorkFlowRepository = classLiveWorkFlowRepository;
            _classLiveCalendarRepository = classLiveCalendarRepository;
            _authContext = authContext;
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
            var classLiveWorkFlow = await _classLiveWorkFlowRepository.Queryable.FirstOrDefaultAsync(x => x.TeacherId == teacherId && x.ClassLiveCalendarId == request.ClassLiveCalendarId, cancellationToken);
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
                else if (request.Type == EnumWorkFlowType.CancelSchedule)
                {
                    status = EnumWorkFlowCancelScheduleStatus.RequestCancel.ToString();
                    DateTime dateTime = DateTime.Now;
                    var learnAgainDate = classLiveCalendar.LiveDate.AddDays(2);// 5 7/7/2023   12 7/7/2023
                    var check = dateTime > classLiveCalendar.LiveDate.AddHours(-24) && dateTime < classLiveCalendar.LiveDate.AddHours(-1);
                    if (dateTime > learnAgainDate || !check)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumClassLiveWorkFlowErrorCode.CanNotCancelLiveTime));
                        return methodResult;
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
