// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.ScheduleQuery
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Training.Application.Services.SystemServices;
    using Fsel.Training.Application.Services.UserServices;
    using Fsel.Training.Domain.Enums.ErrorCodes;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.EntityModels;
    using Fsel.Training.Domain.Models.QueryModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetTeacherLiveDateQuery : GetTeacherLiveDateQueryModel, IRequest<MethodResult<TeacherFreeDateModel>>
    {
    }

    public class GetTeacherLiveDateQueryHandler : IRequestHandler<GetTeacherLiveDateQuery, MethodResult<TeacherFreeDateModel>>
    {
        private readonly ITeacherFreeTimeRepository _teacherFreeTimeRepository;
        private readonly ITeacherFreeDateRepository _teacherFreeDateRepository;
        private readonly IClassRepository _classRepository;
        private readonly ISystemService _systemService;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public GetTeacherLiveDateQueryHandler(ITeacherFreeTimeRepository teacherFreeTimeRepository, IClassRepository classRepository, ISystemService systemService, IUserService userService, ITeacherFreeDateRepository teacherFreeDateRepository, IMapper mapper)
        {
            _teacherFreeTimeRepository = teacherFreeTimeRepository;
            _classRepository = classRepository;
            _systemService = systemService;
            _userService = userService;
            _teacherFreeDateRepository = teacherFreeDateRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<TeacherFreeDateModel>> Handle(GetTeacherLiveDateQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<TeacherFreeDateModel> methodResult = new MethodResult<TeacherFreeDateModel>();

            var @class = await _classRepository.GetByIdAsync(request.ClassId);

            if (@class == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassErrorCode.ClassesNotExits), nameof(@class));
                return methodResult;
            }
            if (@class.TeacherId.HasValue)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassErrorCode.ClassesNotExits), nameof(@class));
                return methodResult;
            }
            if (@class.StartDate == null || @class.EndDate == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassErrorCode.ClassesNotExits), nameof(@class));
                return methodResult;
            }
            if (@class.LiveDays == null || @class.LiveDays.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassErrorCode.ClassesNotExits), nameof(@class));
                return methodResult;
            }

            var teacherFreeDates = await _teacherFreeDateRepository.Queryable
                                    .Include(x => x.TeacherFreeTimes)
                                    .Where(x => x.StartDate.Date <= @class.StartDate.Value.Date && x.EndDate.Date >= @class.EndDate.Value.Date)
                                    .ToArrayAsync(cancellationToken);

            var teacherFreeDate = teacherFreeDates.FirstOrDefault(x => x.TeacherFreeTimes.All(n => @class.LiveDays.Contains(n.DayOfWeek) && @class.LiveTimeFrameId == n.LiveTimeFrameId));
            var teacherFreeDateModel = _mapper.Map<TeacherFreeDateModel>(teacherFreeDate);

            var teacherResult = await _userService.GetTeacherByIdAsync(teacherFreeDateModel.TeacherId);
            var teacher = teacherResult.Content?.Result;
            teacherFreeDateModel.TeacherName = teacher?.Human?.FullName;

            methodResult.Result = teacherFreeDateModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
