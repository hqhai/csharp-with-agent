// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.Schedule
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Application.Services.UserServices.Models;
    using Fsel.Training.Application.Queries.ClassQuery;
    using Fsel.Training.Application.Services.CourseServices;
    using Fsel.Training.Application.Services.SystemServices;
    using Fsel.Training.Application.Services.UserServices;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.EntityModels;
    using Fsel.Training.Domain.Models.QueryModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchManageTeacherLiveScheduleQuery : SearchManageTeacherLiveScheduleQueryModel, IRequest<MethodResult<PagingItemsModel<ClassLiveCalendarModel>>>
    {
    }

    public class SearchManageTeacherLiveScheduleQueryHandler : IRequestHandler<SearchManageTeacherLiveScheduleQuery, MethodResult<PagingItemsModel<ClassLiveCalendarModel>>>
    {
        private readonly IClassLiveCalendarRepository _classLiveCalendar;
        private readonly IUserService _userService;
        private readonly ISystemService _systemService;

        public SearchManageTeacherLiveScheduleQueryHandler(IClassLiveCalendarRepository classLiveCalendar, IUserService userService, ISystemService systemService)
        {
            _classLiveCalendar = classLiveCalendar;
            _userService = userService;
            _systemService = systemService;
        }

        public async Task<MethodResult<PagingItemsModel<ClassLiveCalendarModel>>> Handle(SearchManageTeacherLiveScheduleQuery request, CancellationToken cancellationToken)
        {
            MethodResult<PagingItemsModel<ClassLiveCalendarModel>> methodResult = new MethodResult<PagingItemsModel<ClassLiveCalendarModel>>();
            ArgumentNullException.ThrowIfNull(request);
            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
            var classLiveCalenderQuery = _classLiveCalendar.Queryable
                                        .Include(x => x.Class)
                                        .Select(x => new ClassLiveCalendarModel
                                        {
                                            Id = x.Id,
                                            LiveTimeFrameId = x.LiveTimeFrameId,
                                            Status = x.Status,
                                            ClassCode = x.Class!.Code,
                                            ClassName = x.Class.Name,
                                            TeacherId = x.Class.TeacherId,
                                            StartTime = x.Class.StartTime
                                        });
            int totalItem = await classLiveCalenderQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await classLiveCalenderQuery
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            var teacherResult = await _userService.GetTeacherByIdsAsync(new GetTeacherByIdsQueryModel { Ids = lists.Select(x => x.TeacherId ?? default).Distinct().ToList() });
            var teachers = teacherResult.Content?.Result;

            foreach (var item in lists)
            {
                var teacher = teachers!.FirstOrDefault(x => x.Id == item.TeacherId);
                item.TeacherName = teacher?.Human?.FullName;
            }
            var timeFrameResult = await _systemService.GetTimeFramByIdsAsync(classLiveCalenderQuery.Select(x => x.LiveTimeFrameId).ToList());
            var timeFrames = timeFrameResult.Content?.Result;

            foreach (var item in lists)
            {
                item.TimeFrameEndTime = timeFrames!.EndTime;
                item.TimeFrameStartTime = timeFrames!.StartTime;
            }
            methodResult.Result = new PagingItemsModel<ClassLiveCalendarModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
