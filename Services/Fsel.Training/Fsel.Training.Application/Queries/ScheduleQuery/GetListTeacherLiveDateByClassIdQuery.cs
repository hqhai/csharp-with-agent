// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.ScheduleQuery
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Training.Application.Services.UserServices;
    using Fsel.Training.Application.Services.UserServices.Models;
    using Fsel.Training.Domain.Enums.ErrorCodes;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.EntityModels;
    using Fsel.Training.Domain.Models.QueryModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetListTeacherLiveDateByClassIdQuery : GetTeacherLiveDateByClassIdQueryModel, IRequest<MethodResult<IList<TeacherFreeDateModel>>>
    {
    }

    public class GetListTeacherLiveDateByClassIdQueryHandler : IRequestHandler<GetListTeacherLiveDateByClassIdQuery, MethodResult<IList<TeacherFreeDateModel>>>
    {
        private readonly ITeacherFreeDateRepository _teacherFreeDateRepository;
        private readonly IClassRepository _classRepository;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public GetListTeacherLiveDateByClassIdQueryHandler(ITeacherFreeDateRepository teacherFreeDateRepository
            , IClassRepository classRepository
            , IUserService userService
            , IMapper mapper)
        {
            _teacherFreeDateRepository = teacherFreeDateRepository;
            _classRepository = classRepository;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<TeacherFreeDateModel>>> Handle(GetListTeacherLiveDateByClassIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<TeacherFreeDateModel>> methodResult = new MethodResult<IList<TeacherFreeDateModel>>();

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

            var teacherFreeDates = await _teacherFreeDateRepository.Queryable
                                    .Include(x => x.TeacherFreeTimes)
                                    .Where(x => x.StartDate.Date <= @class.StartDate.Value.Date && x.EndDate.Date >= @class.EndDate.Value.Date && (!@class.TeacherId.HasValue || x.TeacherId != @class.TeacherId))
                                    .ToListAsync(cancellationToken);
            if (teacherFreeDates == null || teacherFreeDates.Count == 0)
            {
                methodResult.Result = null;
                return methodResult;
            }
            var teacherFreeDateOne = teacherFreeDates.Where(x => @class.LiveDays.All(n => x.TeacherFreeTimes.Any(x => x.Priority = true && x.DayOfWeek == n && x.LiveTimeFrameId == @class.LiveTimeFrameId))).ToList();
            var teacherFreeDateOneModel = _mapper.Map<IList<TeacherFreeDateModel>>(teacherFreeDateOne);
            teacherFreeDateOneModel.ForEach(x => x.Priority = true);

            var teacherFreeDateTwo = teacherFreeDates.Where(x => @class.LiveDays.All(n => x.TeacherFreeTimes.Any(x => x.Priority = false && x.DayOfWeek == n && x.LiveTimeFrameId == @class.LiveTimeFrameId))).ToList();
            var teacherFreeDateTwoModel = _mapper.Map<IList<TeacherFreeDateModel>>(teacherFreeDateTwo);
            teacherFreeDateTwoModel.ForEach(x => x.Priority = false);

            var teacherFreeDateModel = teacherFreeDateOneModel.Union(teacherFreeDateTwoModel).ToList();

            var teacherResult = await _userService.GetTeacherByIdsAsync(new GetTeacherByIdsQueryModel { Ids = teacherFreeDateModel.Select(x => x.TeacherId).ToList() });

            var teacher = teacherResult.Content?.Result;
            foreach (var item in teacherFreeDateModel)
            {
                var human = teacher?.FirstOrDefault(x => x.Id == item.TeacherId)?.Human;
                item.TeacherName = human?.FullName;
                item.TeacherCode = human?.Code;
            }

            methodResult.Result = teacherFreeDateModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
