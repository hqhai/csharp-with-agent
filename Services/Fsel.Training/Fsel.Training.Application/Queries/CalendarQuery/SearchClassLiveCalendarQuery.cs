// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.CalendarQuery
{
    using System.Linq.Dynamic.Core;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Shared.Enums;
    using Fsel.Training.Application.Services.CourseServices;
    using Fsel.Training.Application.Services.SystemServices;
    using Fsel.Training.Application.Services.UserServices;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchClassLiveCalendarQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<ClassLiveCalendarSearchModel>>>
    {
    }

    public class SearchClassLiveCalendarQueryHandler : IRequestHandler<SearchClassLiveCalendarQuery, MethodResult<PagingItemsModel<ClassLiveCalendarSearchModel>>>
    {
        private readonly IClassLiveCalendarRepository _classLiveCalendarRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly ICourseService _courseService;
        private readonly ISystemService _systemService;

        public SearchClassLiveCalendarQueryHandler(IClassLiveCalendarRepository classLiveCalendarRepository,
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

        public async Task<MethodResult<PagingItemsModel<ClassLiveCalendarSearchModel>>> Handle(SearchClassLiveCalendarQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<ClassLiveCalendarSearchModel>>();
            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
            var teacherResult = await _userService.GetTeacherByUserIdAsync(_authContext.CurrentUserId);
            var teacherId = teacherResult.Content?.Result?.Id;
            var timeFramesResult = await _systemService.GetLiveTimeFramesAsync();
            var timeFrames = timeFramesResult.Content?.Result;
            var query = _classLiveCalendarRepository.Queryable
                                    .Include(x => x.Class)
                                    .Include(x => x.ClassLiveWorkFlows)
                                    .Where(x => x.Class != null && x.TeacherId == teacherId
                                            && (x.ClassLiveWorkFlows == null
                                            || x.ClassLiveWorkFlows.Count == 0
                                            || (x.ClassLiveWorkFlows.Any(y => y.Type != EnumWorkFlowType.CancelSchedule)
                                            && x.ClassLiveWorkFlows.Any(y => y.Type != EnumWorkFlowType.ChangeTeacher))
                                                ) && x.Status == EnumClassLiveCalendarStatus.NotStudied
                                            )
                                    .AsNoTracking()
                                    .AsEnumerable()
                                    .Where(x => timeFrames != null && timeFrames.FirstOrDefault(y => y.Id == x.LiveTimeFrameId) != null &&
                                    x.LiveDate.Date.AddHours(timeFrames.FirstOrDefault(y => y.Id == x.LiveTimeFrameId)!.EndTime ?? 0) > DateTime.UtcNow)
                                    .Select(x => new ClassLiveCalendarSearchModel
                                    {
                                        Id = x.Id,
                                        CreatedDate = x.CreatedDate,
                                        CourseId = x.Class!.CourseId,
                                        Code = x.Class!.Code,
                                        AccessLink = x.AccessLink,
                                        LiveTimeFrameId = x.LiveTimeFrameId,
                                        LiveDate = x.LiveDate
                                    });

            int totalItem = query.Count();
            var lists = query.ApplySortAndPaging(request).ToList();
            var courseIds = lists.Select(x => x.CourseId).Distinct().ToList();
            var courseResults = await _courseService.GetListCourseByIds(courseIds);
            var courses = courseResults.Content?.Result;

            foreach (var item in lists)
            {
                var liveTimeFrame = timeFrames?.FirstOrDefault(x => x.Id == item.LiveTimeFrameId);
                item.CourseLevel = courses?.FirstOrDefault(x => x.Id == item.CourseId)?.CourseLevel ?? default;
                item.StartTime = liveTimeFrame?.StartTime;
                item.EndTime = liveTimeFrame?.EndTime;

                DateTime dateTime = DateTime.UtcNow;
                var dateTimeNow = dateTime.Date.AddHours(dateTime.Hour).AddMinutes(dateTime.Minute);

                var assignTeacher = item.LiveDate.Date.AddHours(item.StartTime ?? 0);
                var endTime = assignTeacher.AddHours(-1);

                item.IsActiveWorkFlow = assignTeacher > dateTimeNow.AddHours(24);
                item.IsActiveWorkPlan = dateTimeNow > assignTeacher.AddHours(-24) && dateTimeNow < endTime;
                item.IsActiveCalendar = dateTimeNow >= assignTeacher && dateTimeNow < assignTeacher.AddHours(1);
            }
            methodResult.Result = new PagingItemsModel<ClassLiveCalendarSearchModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
