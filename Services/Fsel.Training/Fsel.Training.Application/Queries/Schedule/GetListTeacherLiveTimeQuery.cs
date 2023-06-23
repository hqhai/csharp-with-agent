// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.Schedule
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Application.Services.UserServices.Models;
    using Fsel.Training.Application.Services.SystemServices;
    using Fsel.Training.Application.Services.UserServices;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetListTeacherLiveTimeQuery : IRequest<MethodResult<IList<TeacherFreeTimeModel>>>
    {
    }

    public class GetListTeacherLiveTimeQueryHandler : IRequestHandler<GetListTeacherLiveTimeQuery, MethodResult<IList<TeacherFreeTimeModel>>>
    {
        private readonly ITeacherFreeTimeRepository _teacherFreeTimeRepository;
        private readonly ISystemService _systemService;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public GetListTeacherLiveTimeQueryHandler(ITeacherFreeTimeRepository teacherFreeTimeRepository, ISystemService systemService, IMapper mapper, IUserService userService)
        {
            _teacherFreeTimeRepository = teacherFreeTimeRepository;
            _systemService = systemService;
            _mapper = mapper;
            _userService = userService;
        }

        public async Task<MethodResult<IList<TeacherFreeTimeModel>>> Handle(GetListTeacherLiveTimeQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            MethodResult<IList<TeacherFreeTimeModel>> methodResult = new MethodResult<IList<TeacherFreeTimeModel>>();

            var teacherFreeTimeQuery = await _teacherFreeTimeRepository.Queryable
                                    .Include(x => x.TeacherFreeDate)
                                    .Select(x => new TeacherFreeTimeModel
                                    {
                                        Id = x.Id,
                                        LiveTimeFrameId = x.LiveTimeFrameId,
                                        DayOfWeek = x.DayOfWeek,
                                        TeacherId = x.TeacherFreeDate!.TeacherId,
                                    }).ToListAsync(cancellationToken);
            var teacherResult = await _userService.GetTeacherByIdsAsync(new GetTeacherByIdsQueryModel { Ids = teacherFreeTimeQuery.Select(x => x.TeacherId).Distinct().ToList() });
            var teachers = teacherResult.Content?.Result;

            foreach (var item in teacherFreeTimeQuery)
            {
                var teacher = teachers!.FirstOrDefault(x => x.Id == item.TeacherId);
                item.TeacherName = teacher?.Human?.FullName;
            }
            var timeFrameResult = await _systemService.GetTimeFramByIdsAsync(teacherFreeTimeQuery.Select(x => x.LiveTimeFrameId).ToList());
            var timeFrames = timeFrameResult.Content?.Result;

            foreach (var item in teacherFreeTimeQuery)
            {
                item.TimeFrameEndTime = timeFrames!.EndTime;
                item.TimeFrameStartTime = timeFrames!.StartTime;
            }
            methodResult.Result = teacherFreeTimeQuery;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
