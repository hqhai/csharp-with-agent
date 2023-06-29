// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.CalendarQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Training.Application.Services.CourseServices;
    using Fsel.Training.Application.Services.SystemServices;
    using Fsel.Training.Application.Services.UserServices;
    using Fsel.Training.Domain.Entities;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.EntityModels;
    using Fsel.Training.Domain.Models.QueryModels.CalendarQuery;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchCalendarByTeacherIdQuery : SearchCalendarByTeacherIdQueryModel, IRequest<MethodResult<PagingItemsModel<ClassLiveCalendarSearchModel>>>
    {
    }

    public class SearchCalendarByTeacherIdQueryHandler : IRequestHandler<SearchCalendarByTeacherIdQuery, MethodResult<PagingItemsModel<ClassLiveCalendarSearchModel>>>
    {
        private readonly IClassLiveCalendarRepository _classLiveCalendarRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly ICourseService _courseService;
        private readonly ISystemService _systemService;

        public SearchCalendarByTeacherIdQueryHandler(IClassLiveCalendarRepository classLiveCalendarRepository,
            AuthContext authContext,
            IUserService userService,
            ICourseService courseService,
            ISystemService systemService)
        {
            _classLiveCalendarRepository = classLiveCalendarRepository;
            _authContext = authContext;
            _userService = userService;
            _courseService = courseService;
            _systemService = systemService;
        }

        public async Task<MethodResult<PagingItemsModel<ClassLiveCalendarSearchModel>>> Handle(SearchCalendarByTeacherIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<ClassLiveCalendarSearchModel>>();
            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
            var teacherResult = await _userService.GetTeacherByUserIdAsync(_authContext.CurrentUserId);
            var teacher = teacherResult.Content?.Result;
            var query = _classLiveCalendarRepository.Queryable
                                    .Include(x => x.Class)
                                    .Where(x => x.Class!.TeacherId == teacher!.Id)
                                    .Select(x => GetByClass(x));

            int totalItem = await query.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await query
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            var courseIds = lists.Select(x => x.CourseId).ToList();
            var courseResults = await _courseService.GetListCourseByIds(courseIds);
            var courses = courseResults.Content?.Result;
            var timeFramesResult = await _systemService.GetLiveTimeFramesAsync();
            var timeFrames = timeFramesResult.Content?.Result;
            foreach (var item in lists)
            {
                var liveTimeFrame = timeFrames?.FirstOrDefault(x => x.Id == item.LiveTimeFrameId);
                item.CourseLevel = courses?.FirstOrDefault(x => x.Id == item.CourseId)?.CourseLevel ?? default;
                item.StartTime = liveTimeFrame?.StartTime;
                item.EndTime = liveTimeFrame?.EndTime;
            }
            methodResult.Result = new PagingItemsModel<ClassLiveCalendarSearchModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        public ClassLiveCalendarSearchModel GetByClass(ClassLiveCalendar classLiveCalendar)
        {
            ArgumentNullException.ThrowIfNull(classLiveCalendar);
            var @class = classLiveCalendar.Class;
            return new ClassLiveCalendarSearchModel
            {
                Id = classLiveCalendar.Id,
                CreatedDate = classLiveCalendar.CreatedDate,
                CourseId = @class?.CourseId ?? default,
                Code = @class?.Code,
                AccessLink = classLiveCalendar.AccessLink,
                LiveTimeFrameId = @class?.LiveTimeFrameId ?? default,
                LiveDate = classLiveCalendar.LiveDate
            };
        }

        //public ClassLiveCalendarSearchModel GetByWorkFlow(ClassLiveCalendar classLiveCalendar)
        //{
        //    ArgumentNullException.ThrowIfNull(classLiveCalendar);
        //    var @class = classLiveCalendar.Class;
        //    return new ClassLiveCalendarSearchModel
        //    {
        //        Id = classLiveCalendar.Id,
        //        CreatedDate = classLiveCalendar.CreatedDate,
        //        CourseId = @class?.CourseId ?? default,
        //        Code = @class?.Code,
        //        AccessLink = classLiveCalendar.AccessLink,
        //        DayOfWeeks = @class?.LiveDays,
        //        StartDate = @class?.StartDate,
        //        EndDate = @class?.EndDate,
        //        LiveTimeFrameId = @class?.LiveTimeFrameId ?? default,
        //        LiveDate = classLiveCalendar.LiveDate
        //    };
        //}
    }
}
