// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.ScheduleQuery
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Training.Application.Services.UserServices;
    using Fsel.Training.Domain.Entities;
    using Fsel.Training.Domain.Enums.ErrorCodes;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.EntityModels;
    using Fsel.Training.Domain.Models.QueryModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetTeacherLiveDateByClassIdQuery : GetTeacherLiveDateByClassIdQueryModel, IRequest<MethodResult<TeacherFreeDateModel>>
    {
    }

    public class GetTeacherLiveDateByClassIdQueryHandler : IRequestHandler<GetTeacherLiveDateByClassIdQuery, MethodResult<TeacherFreeDateModel>>
    {
        private readonly ITeacherFreeDateRepository _teacherFreeDateRepository;
        private readonly IClassRepository _classRepository;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public GetTeacherLiveDateByClassIdQueryHandler(ITeacherFreeDateRepository teacherFreeDateRepository
            , IClassRepository classRepository
            , IUserService userService
            , IMapper mapper)
        {
            _teacherFreeDateRepository = teacherFreeDateRepository;
            _classRepository = classRepository;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<MethodResult<TeacherFreeDateModel>> Handle(GetTeacherLiveDateByClassIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<TeacherFreeDateModel> methodResult = new MethodResult<TeacherFreeDateModel>();

            var @class = await _classRepository.GetByIdAsync(request.ClassId);

            if (@class == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(@class));
                return methodResult;
            }

            if (@class.StartDate == null || @class.EndDate == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassErrorCode.ClassEndDateAndStartDateIsNull), nameof(@class));
                return methodResult;
            }
            if (@class.LiveDays == null || @class.LiveDays.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(@class.LiveDays));
                return methodResult;
            }
            var teacherFreeDate = new TeacherFreeDate();
            if (@class.TeacherId.HasValue)
            {
                teacherFreeDate = await _teacherFreeDateRepository.Queryable
                                   .Include(x => x.TeacherFreeTimes)
                                   .FirstOrDefaultAsync(x => x.TeacherId == @class.TeacherId, cancellationToken);
            }
            else
            {
                var teacherFreeDates = await _teacherFreeDateRepository.Queryable
                                   .Include(x => x.TeacherFreeTimes)
                                   .Where(x => x.StartDate.Date <= @class.StartDate.Value.Date && x.EndDate.Date >= @class.EndDate.Value.Date)
                                   .ToArrayAsync(cancellationToken);
                if (teacherFreeDates == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(teacherFreeDates));
                    return methodResult;
                }

                teacherFreeDate = teacherFreeDates.FirstOrDefault(x => @class.LiveDays.All(n => x.TeacherFreeTimes.Any(x => x.Priority = true && x.DayOfWeek == n && x.LiveTimeFrameId == @class.LiveTimeFrameId)));
                if (teacherFreeDate == null)
                {
                    teacherFreeDate = teacherFreeDates.FirstOrDefault(x => @class.LiveDays.All(n => x.TeacherFreeTimes.Any(x => x.Priority = false && x.DayOfWeek == n && x.LiveTimeFrameId == @class.LiveTimeFrameId)));
                }
            }

            if (teacherFreeDate == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(teacherFreeDate));
                return methodResult;
            }
            var teacherFreeDateModel = _mapper.Map<TeacherFreeDateModel>(teacherFreeDate);
            var teacherResult = await _userService.GetTeacherByIdAsync(teacherFreeDateModel.TeacherId);
            var teacher = teacherResult.Content?.Result;
            teacherFreeDateModel.TeacherName = teacher?.Human?.FullName;
            teacherFreeDateModel.TeacherCode = teacher?.Human?.Code;

            methodResult.Result = teacherFreeDateModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
