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
        private readonly IMediator _mediator;

        public GetTeacherLiveDateByClassIdQueryHandler(ITeacherFreeDateRepository teacherFreeDateRepository
            , IClassRepository classRepository
            , IUserService userService
            , IMapper mapper,
IMediator mediator)
        {
            _teacherFreeDateRepository = teacherFreeDateRepository;
            _classRepository = classRepository;
            _userService = userService;
            _mapper = mapper;
            _mediator = mediator;
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
                var teacherFreeDatesResult = await _mediator.Send(new GetListTeacherLiveDateByClassIdQuery { ClassId = request.ClassId }, cancellationToken);
                var teacherFreeDates = teacherFreeDatesResult.Result;
                if (teacherFreeDates != null)
                {
                    methodResult.Result = teacherFreeDates.FirstOrDefault();
                    methodResult.StatusCode = StatusCodes.Status200OK;
                    return methodResult;
                }
            }

            if (teacherFreeDate == null)
            {
                methodResult.Result = null;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var teacherFreeDateModel = _mapper.Map<TeacherFreeDateModel>(teacherFreeDate);
            var teacherResult = await _userService.GetTeacherByIdAsync(teacherFreeDateModel.TeacherId);
            var teacher = teacherResult.Content?.Result;
            teacherFreeDateModel.TeacherName = teacher?.User?.FullName;
            teacherFreeDateModel.TeacherCode = teacher?.User?.Code;

            methodResult.Result = teacherFreeDateModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
