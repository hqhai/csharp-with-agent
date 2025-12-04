// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.ScheduleQuery
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Shared.Enums;
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
        private readonly IClassRepository _classRepository;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly ITeacherFreeTimeLiveRepository _teacherFreeTimeLiveRepository;

        public GetListTeacherLiveDateByClassIdQueryHandler(IClassRepository classRepository
            , IUserService userService
            , IMapper mapper,
ITeacherFreeTimeLiveRepository teacherFreeTimeLiveRepository)
        {
            _classRepository = classRepository;
            _userService = userService;
            _mapper = mapper;
            _teacherFreeTimeLiveRepository = teacherFreeTimeLiveRepository;
        }

        public async Task<MethodResult<IList<TeacherFreeDateModel>>> Handle(GetListTeacherLiveDateByClassIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<TeacherFreeDateModel>> methodResult = new MethodResult<IList<TeacherFreeDateModel>>();

            var @class = await _classRepository.Queryable.Include(x => x.ClassLiveCalendars).FirstOrDefaultAsync(p => p.Id == request.ClassId, cancellationToken);

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

            var classLiveCalendars = @class.ClassLiveCalendars.Where(n => n.Status == EnumClassLiveCalendarStatus.NotStudied);

            var teacherFreeTimeLives = _teacherFreeTimeLiveRepository.Queryable.Include(n => n.TeacherFreeTime).ThenInclude(m => m!.TeacherFreeDate)
            .AsEnumerable()
            .Where(p => classLiveCalendars.Any(x => x.LiveDate.Date == p.LiveDate.Date && x.LiveTimeFrameId == p.TeacherFreeTime?.LiveTimeFrameId && !p.IsUsed))
            .ToList();

            var teacherFreeDates = teacherFreeTimeLives.Select(p => p.TeacherFreeTime).Select(x => x!.TeacherFreeDate).Distinct().ToList();

            var teacherFreeTimeLiveDates = teacherFreeTimeLives.GroupBy(m => m.LiveDate).OrderBy(n => n.Key).ToList();

            if (classLiveCalendars.Count() != teacherFreeTimeLiveDates.Count)
            {
                methodResult.Result = null;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var groupByTeacherFreeDates = teacherFreeDates.GroupBy(p => p?.TeacherId).ToList();

            foreach (var teacherFreeDate in groupByTeacherFreeDates)
            {
                int countDay = 0;
                foreach (var teacherFreeTimeLiveDate in teacherFreeTimeLiveDates)
                {
                    if (teacherFreeDate.SelectMany(n => n!.TeacherFreeTimes).Any(p => teacherFreeTimeLiveDate.Select(x => x.TeacherFreeTimeId).Contains(p.Id)))
                    {
                        countDay++;
                    }
                }
                if (countDay != teacherFreeTimeLiveDates.Count)
                {
                    teacherFreeDates = teacherFreeDates.Where(p => p?.TeacherId != teacherFreeDate.FirstOrDefault()?.TeacherId).ToList();
                }
            }

            if (teacherFreeDates == null || teacherFreeDates.Count == 0)
            {
                methodResult.Result = null;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            var teacherFreeDateTwo = teacherFreeDates.Where(x => groupByTeacherFreeDates.SelectMany(p => p).Any(m => m?.Id == x?.Id && m!.TeacherFreeTimes.Any(n => !n.Priority))).ToList();
            var teacherFreeDateTwoModel = _mapper.Map<IList<TeacherFreeDateModel>>(teacherFreeDateTwo);
            teacherFreeDateTwoModel.ForEach(x => x.Priority = false);

            var teacherFreeDateOne = teacherFreeDates.Where(x => !teacherFreeDateTwo.Select(m => m?.TeacherId).Contains(x?.TeacherId)).ToList();
            var teacherFreeDateOneModel = _mapper.Map<IList<TeacherFreeDateModel>>(teacherFreeDateOne);
            teacherFreeDateOneModel.ForEach(x => x.Priority = true);

            var teacherFreeDateModel = teacherFreeDateOneModel.Union(teacherFreeDateTwoModel).ToList();

            teacherFreeDateModel = teacherFreeDateModel.DistinctBy(x => x.TeacherId).OrderByDescending(p => p.Priority).ToList();

            var teacherResult = await _userService.GetTeacherByIdsAsync(new GetTeacherByIdsQueryModel { Ids = teacherFreeDateModel.Select(x => x.TeacherId).ToList() });

            var teachers = teacherResult.Content?.Result;
            foreach (var item in teacherFreeDateModel)
            {
                var teacher = teachers?.FirstOrDefault(x => x.Id == item.TeacherId);
                item.TeacherName = teacher?.User?.FullName;
                item.TeacherCode = teacher?.User?.Code;
            }

            methodResult.Result = teacherFreeDateModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
