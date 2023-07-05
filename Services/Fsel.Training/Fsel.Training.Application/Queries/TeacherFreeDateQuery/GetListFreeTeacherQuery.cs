// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.TeacherFreeDateQuery
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Application.Services.UserServices.Models;
    using Fsel.Training.Application.Services.SystemServices;
    using Fsel.Training.Application.Services.UserServices;
    using Fsel.Training.Domain.Enums.ErrorCodes;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetListFreeTeacherByCalendarQuery : IRequest<MethodResult<IList<TeacherFreeDateModel>>>
    {
        public Guid ClassLiveCalendarId { get; set; }
    }

    public class GetListFreeTeacherByCalendarQueryHandler : IRequestHandler<GetListFreeTeacherByCalendarQuery, MethodResult<IList<TeacherFreeDateModel>>>
    {
        private readonly ITeacherFreeDateRepository _teacherFreeDateRepository;
        private readonly IClassLiveCalendarRepository _classLiveCalendarRepository;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly ISystemService _systemService;

        public GetListFreeTeacherByCalendarQueryHandler(ITeacherFreeDateRepository teacherFreeDateRepository
            , IClassLiveCalendarRepository classLiveCalendarRepository
            , IUserService userService
            , ISystemService systemService
            , IMapper mapper)
        {
            _teacherFreeDateRepository = teacherFreeDateRepository;
            _classLiveCalendarRepository = classLiveCalendarRepository;
            _userService = userService;
            _mapper = mapper;
            _systemService = systemService;

        }

        public async Task<MethodResult<IList<TeacherFreeDateModel>>> Handle(GetListFreeTeacherByCalendarQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<TeacherFreeDateModel>>();
            var classLiveLesson = await _classLiveCalendarRepository.GetByIdAsync(request.ClassLiveCalendarId);

            if (classLiveLesson == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassLiveCalendarErrorCode.ClassLiveCalendarNotExits));
                return methodResult;
            }

            var listClassLiveTime = await _systemService.GetLiveTimeFramesAsync();
            var lstLiveTimeFrame = listClassLiveTime?.Content?.Result;
            var matchingLiveTimeFrame = lstLiveTimeFrame != null ? lstLiveTimeFrame.FirstOrDefault(x => x.Id == classLiveLesson.LiveTimeFrameId) : null;
            var dayOfWeekRequest = classLiveLesson.LiveDate.DayOfWeek;
            var teacherFreeDates = await _teacherFreeDateRepository.Queryable
                               .Include(x => x.TeacherFreeTimes)
                               .Where(x => x.StartDate.Date <= classLiveLesson.LiveDate && x.EndDate.Date >= classLiveLesson.LiveDate)
                               .ToListAsync(cancellationToken);

            var teacherFreeDate = teacherFreeDates.Where(x => x.TeacherFreeTimes
                                                            .Any(n => lstLiveTimeFrame
                                                                      .Any(clt => clt.Id == n.LiveTimeFrameId
                                                                      && (clt.StartTime > matchingLiveTimeFrame.EndTime || clt.EndTime < matchingLiveTimeFrame.StartTime)
                                                                      )
                                                                      && n.DayOfWeek == dayOfWeekRequest
                                                                 )
                                                        ).ToList();

            var teacherFreeDateModel = _mapper.Map<IList<TeacherFreeDateModel>>(teacherFreeDate);
            var teacherResult = await _userService.GetTeacherByIdsAsync(new GetTeacherByIdsQueryModel { Ids = teacherFreeDateModel.Select(x => x.TeacherId).ToList() });
            var teacher = teacherResult.Content?.Result;
            foreach (var item in teacherFreeDateModel)
            {
                item.TeacherName = teacher?.FirstOrDefault(x => x.Id == item.TeacherId)?.Human?.FullName;
                item.TeacherCode = teacher?.FirstOrDefault(x => x.Id == item.TeacherId)?.Human?.Code;
            }

            methodResult.Result = teacherFreeDateModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
