// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.Schedule
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Application.Services.UserServices.Models;
    using Fsel.Training.Application.Services.SystemServices;
    using Fsel.Training.Application.Services.UserServices;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.EntityModels;
    using Fsel.Training.Domain.Models.QueryModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchTeacherFreeTimeByCsoQuery : SearchTeacherFreeTimeByCsoQueryModel, IRequest<MethodResult<PagingItemsModel<TeacherFreeTimeModel>>>
    {
    }

    public class SearchTeacherFreeTimeByCsoQueryHandler : IRequestHandler<SearchTeacherFreeTimeByCsoQuery, MethodResult<PagingItemsModel<TeacherFreeTimeModel>>>
    {
        private readonly ITeacherFreeTimeRepository _teacherFreeTimeRepository;
        private readonly IUserService _userService;
        private readonly ISystemService _systemService;

        public SearchTeacherFreeTimeByCsoQueryHandler(ITeacherFreeTimeRepository teacherFreeTimeRepository, IUserService userService, ISystemService systemService)
        {
            _teacherFreeTimeRepository = teacherFreeTimeRepository;
            _userService = userService;
            _systemService = systemService;
        }

        public async Task<MethodResult<PagingItemsModel<TeacherFreeTimeModel>>> Handle(SearchTeacherFreeTimeByCsoQuery request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<PagingItemsModel<TeacherFreeTimeModel>>();
            ArgumentNullException.ThrowIfNull(request);
            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var priorityCalenderQuery = _teacherFreeTimeRepository.Queryable
                                    .Include(x => x.TeacherFreeDate)
                                    .Select(x => new TeacherFreeTimeModel
                                    {
                                        Id = x.Id,
                                        TeacherFreeDateId = x.TeacherFreeDateId,
                                        CreatedDate = x.CreatedDate,
                                        DayOfWeek = x.DayOfWeek,
                                        Priority = x.Priority,
                                        TeacherId = x.TeacherFreeDate!.TeacherId,
                                        StartTime = x.TeacherFreeDate.StartTime,
                                        EndTime = x.TeacherFreeDate.EndTime,
                                    });
            if (request.StartTime != null)
            {
                priorityCalenderQuery = priorityCalenderQuery.Where(m => m.StartTime == request.StartTime);
            }
            if (request.EndTime != null)
            {
                priorityCalenderQuery = priorityCalenderQuery.Where(m => m.EndTime == request.EndTime);
            }
            if (request.Priority != null)
            {
                priorityCalenderQuery = priorityCalenderQuery.Where(m => m.Priority == request.Priority);
            }
            if (request.DayOfWeek != null)
            {
                priorityCalenderQuery = priorityCalenderQuery.Where(m => m.DayOfWeek == request.DayOfWeek);
            }

            int totalItem = await priorityCalenderQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await priorityCalenderQuery
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            var teacherResult = await _userService.GetTeacherByIdsAsync(new GetTeacherByIdsQueryModel { Ids = lists.Select(x => x.TeacherId).Distinct().ToList() });
            var teachers = teacherResult.Content?.Result;

            foreach (var item in lists)
            {
                var teacher = teachers!.FirstOrDefault(x => x.Id == item.TeacherId);
                item.TeacherName = teacher?.Human?.FullName;
            }
            var timeFrameResult = await _systemService.GetTimeFramByIdsAsync(priorityCalenderQuery.Select(x => x.LiveTimeFrameId).ToList());
            var timeFrames = timeFrameResult.Content?.Result;

            foreach (var item in lists)
            {
                item.TimeFrameEndTime = timeFrames!.EndTime;
                item.TimeFrameStartTime = timeFrames!.StartTime;
            }

            /*if (request.TimeFrameStartTime != null)
            {
                timeFrames = .Where(m => m.StartTime == request.StartTime);
            }*/
            methodResult.Result = new PagingItemsModel<TeacherFreeTimeModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
