// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.Schedule
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Application.Services.UserServices.Models;
    using Fsel.Training.Application.Services.SystemServices;
    using Fsel.Training.Application.Services.SystemServices.Model;
    using Fsel.Training.Application.Services.UserServices;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.EntityModels;
    using Fsel.Training.Domain.Models.QueryModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchTeacherFreeDateByCsoQuery : SearchTeacherFreeDatesByCsoQueryModel, IRequest<MethodResult<IList<TeacherFreeTimeModel>>>
    {
    }

    public class SearchTeacherFreeDateByCsoQueryHandler : IRequestHandler<SearchTeacherFreeDateByCsoQuery, MethodResult<IList<TeacherFreeTimeModel>>>
    {
        private readonly ITeacherFreeTimeRepository _teacherFreeTimeRepository;
        private readonly ISystemService _systemService;
        private readonly IUserService _userService;

        public SearchTeacherFreeDateByCsoQueryHandler(ISystemService systemService, IUserService userService, ITeacherFreeTimeRepository teacherFreeTimeRepository)
        {
            _systemService = systemService;
            _userService = userService;
            _teacherFreeTimeRepository = teacherFreeTimeRepository;
        }

        public async Task<MethodResult<IList<TeacherFreeTimeModel>>> Handle(SearchTeacherFreeDateByCsoQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            MethodResult<IList<TeacherFreeTimeModel>> methodResult = new MethodResult<IList<TeacherFreeTimeModel>>();

            var teacherFreeTimeQuery = _teacherFreeTimeRepository.Queryable
                                    .Include(x => x.TeacherFreeDate)
                                    .Where(x => request.TeacherId.HasValue && x.TeacherFreeDate!.TeacherId == request.TeacherId)
                                    .Where(x => request.StartTime.HasValue && x.TeacherFreeDate!.StartTime.Date <= request.StartTime.Value.Date)
                                    .Where(x => request.EndTime.HasValue && x.TeacherFreeDate!.EndTime.Date >= request.EndTime.Value.Date)
                                    .Select(x => new TeacherFreeTimeModel
                                    {
                                        Id = x.Id,
                                        CreatedDate = x.CreatedDate,
                                        LiveTimeFrameId = x.LiveTimeFrameId,
                                        DayOfWeek = x.DayOfWeek
                                    });

            var lists = await teacherFreeTimeQuery
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            var timeFramesResult = await _systemService.GetTimeFrameByIdsAsync(new GetTimeFrameByIdsModel { Ids = teacherFreeTimeQuery.Select(x => x.LiveTimeFrameId).ToList() });
            var timeFrames = timeFramesResult.Content?.Result;

            foreach (var item in lists)
            {
                var timeFrame = timeFrames?.FirstOrDefault(x => x.Id == item.LiveTimeFrameId);
                item.StartTime = timeFrame?.StartTime;
                item.EndTime = timeFrame?.EndTime;
            }

            methodResult.Result = lists;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
