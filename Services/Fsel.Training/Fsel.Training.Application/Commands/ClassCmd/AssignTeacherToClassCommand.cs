// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Commands.ClassCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Training.Application.Services.SystemServices;
    using Fsel.Training.Application.Services.UserServices;
    using Fsel.Training.Domain.Entities;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.CommandModels.Classes;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class AssignTeacherToClassCommand : AssignTeacherToClassCommandModel, IRequest<MethodResult<ClassModel>>
    {
    }

    public class AssignTeacherToClassCommandHandler : IRequestHandler<AssignTeacherToClassCommand, MethodResult<ClassModel>>
    {
        private readonly IClassRepository _classRepository;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly ISystemService _systemService;
        private readonly IClassLiveCalendarRepository _classLiveCalendarRepository;

        public AssignTeacherToClassCommandHandler(IClassRepository classRepository
            , IUserService userService
            , IMapper mapper
            , ISystemService systemService
            , IClassLiveCalendarRepository classLiveCalendarRepository)
        {
            _classRepository = classRepository;
            _userService = userService;
            _mapper = mapper;
            _systemService = systemService;
            _classLiveCalendarRepository = classLiveCalendarRepository;
        }

        public async Task<MethodResult<ClassModel>> Handle(AssignTeacherToClassCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ClassModel> methodResult = new MethodResult<ClassModel>();
            var @class = await _classRepository.GetByIdAsync(request.Id);
            if (@class == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(@class));
                return methodResult;
            }
            var liveTimeFrameResults = await _systemService.GetLiveTimeFramesAsync();
            if (!liveTimeFrameResults.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallSystemServiceError));
                return methodResult;
            }
            var liveTimeFrames = liveTimeFrameResults?.Content?.Result;
            var classLives = await _classLiveCalendarRepository.Queryable.Where(x => x.ClassId == @class.Id && x.TeacherId == @class.TeacherId).ToListAsync(cancellationToken);
            if (classLives != null && classLives.Count > 0)
            {
                List<ClassLiveCalendar> classLiveCalendars = new List<ClassLiveCalendar>();
                foreach (var classLive in classLives)
                {
                    var liveTimeFrame = liveTimeFrames?.FirstOrDefault(x => x.Id == classLive.LiveTimeFrameId);
                    if (liveTimeFrame != null && classLive.LiveDate.Date.AddHours(liveTimeFrame.StartTime ?? default) > DateTime.Now)
                    {
                        classLive.TeacherId = request.TeacherId;
                        classLiveCalendars.Add(classLive);
                    }
                }
                _classLiveCalendarRepository.UpdateList(classLiveCalendars);
                await _classLiveCalendarRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            }

            var teacherResult = await _userService.GetTeacherByIdAsync(request.TeacherId);
            if (!teacherResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(teacherResult));
                return methodResult;
            }
            var teacher = teacherResult?.Content?.Result;
            if (teacher == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(teacher));
                return methodResult;
            }
            @class.Id = request.Id;
            @class.TeacherId = request.TeacherId;

            await _classRepository.ExecuteTransactionAsync(async () =>
            {
                @class.TeacherApprovalStatus = Shared.Enums.EnumTeacherApprovalStatus.Pending;
                _classRepository.Update(@class);
                await _classRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<ClassModel>(@class);
                return methodResult;
            });
            return methodResult;
        }
    }
}
