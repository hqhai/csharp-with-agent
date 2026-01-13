// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.TeacherFreeTimeQuery
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Training.Application.Services.SystemServices;
    using Fsel.Training.Application.Services.UserServices;
    using Fsel.Training.Application.Services.UserServices.Models;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetListTeacherFreeTimeByCalendarQuery : IRequest<MethodResult<IList<TeacherFreeTimeModel>>>
    {
        public Guid ClassLiveCalendarId { get; set; }
    }

    public class GetListTeacherFreeTimeByCalendarQueryHandler : IRequestHandler<GetListTeacherFreeTimeByCalendarQuery, MethodResult<IList<TeacherFreeTimeModel>>>
    {
        private readonly ITeacherFreeTimeRepository _teacherFreeTimeRepository;
        private readonly IClassLiveCalendarRepository _classLiveCalendarRepository;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly ISystemService _systemService;

        public GetListTeacherFreeTimeByCalendarQueryHandler(ITeacherFreeTimeRepository teacherFreeTimeRepository
            , IClassLiveCalendarRepository classLiveCalendarRepository
            , IUserService userService
            , ISystemService systemService
            , IMapper mapper)
        {
            _teacherFreeTimeRepository = teacherFreeTimeRepository;
            _classLiveCalendarRepository = classLiveCalendarRepository;
            _userService = userService;
            _mapper = mapper;
            _systemService = systemService;
        }

        public async Task<MethodResult<IList<TeacherFreeTimeModel>>> Handle(GetListTeacherFreeTimeByCalendarQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<TeacherFreeTimeModel>>();
            var classLiveLesson = await _classLiveCalendarRepository.GetByIdAsync(request.ClassLiveCalendarId);

            if (classLiveLesson == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(classLiveLesson));
                return methodResult;
            }

            var listClassLiveTime = await _systemService.GetLiveTimeFramesAsync();
            var listLiveTimeFrame = listClassLiveTime?.Content?.Result;

            var matchingLiveTimeFrame = listLiveTimeFrame != null ? listLiveTimeFrame.FirstOrDefault(x => x.Id == classLiveLesson.LiveTimeFrameId) : null;
            var dayOfWeekRequest = classLiveLesson.LiveDate.DayOfWeek;

            var teacherFreeTimes = await _teacherFreeTimeRepository.Queryable
                               .Include(x => x.TeacherFreeDate)
                               .Where(x => x.TeacherFreeDate!.StartDate.Date <= classLiveLesson.LiveDate.Date && x.TeacherFreeDate!.EndDate.Date >= classLiveLesson.LiveDate.Date)
                               .Where(x => x.DayOfWeek == classLiveLesson.LiveDate.DayOfWeek && x.LiveTimeFrameId == classLiveLesson.LiveTimeFrameId)
                               .AsNoTracking()
                               .Select(x => new TeacherFreeTimeModel
                               {
                                   Id = x.Id,
                                   DayOfWeek = x.DayOfWeek,
                                   LiveTimeFrameId = x.LiveTimeFrameId,
                                   TeacherId = x.TeacherFreeDate!.TeacherId,
                                   Priority = x.Priority,
                               }).ToListAsync(cancellationToken);

            var classLiveCalendars = await _classLiveCalendarRepository.Queryable
                                  .Where(x => x.LiveDate.Date == classLiveLesson.LiveDate.Date && x.LiveTimeFrameId == classLiveLesson.LiveTimeFrameId)
                                  .ToListAsync(cancellationToken);

            var filteredClassLiveCalendars = classLiveCalendars
                         .Where(x => x.TeacherId != null && x.TeacherId != classLiveLesson.TeacherId && teacherFreeTimes.Any(y => y.TeacherId == x.TeacherId &&
                                                      y.LiveTimeFrameId == x.LiveTimeFrameId &&
                                                      y.DayOfWeek == x.LiveDate.DayOfWeek))
                        .ToList();
            teacherFreeTimes = teacherFreeTimes.Where(x => !filteredClassLiveCalendars.Any(y => y.TeacherId == x.TeacherId)).ToList();

            var teacherFreeTimeModel = _mapper.Map<IList<TeacherFreeTimeModel>>(teacherFreeTimes);
            var teacherResult = await _userService.GetTeacherByIdsAsync(new GetTeacherByIdsQueryModel { Ids = teacherFreeTimeModel.Select(x => x.TeacherId).ToList() });
            var teacher = teacherResult.Content?.Result;
            foreach (var item in teacherFreeTimeModel)
            {
                item.TeacherName = teacher?.FirstOrDefault(x => x.Id == item.TeacherId)?.User?.FullName;
                item.TeacherCode = teacher?.FirstOrDefault(x => x.Id == item.TeacherId)?.User?.Code;
            }

            methodResult.Result = teacherFreeTimeModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
