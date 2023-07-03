// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.ClassLiveQuery
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Training.Application.Services.CourseServices;
    using Fsel.Training.Application.Services.SystemServices;
    using Fsel.Training.Application.Services.UserServices;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.EntityModels;
    using Fsel.Training.Domain.Models.QueryModels.ClassLiveQuery;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchClassLiveByTeacherIdQuery : SearchClassLiveByTeacherIdQueryModel, IRequest<MethodResult<PagingItemsModel<ClassLiveModel>>>
    {
    }

    public class SearchClassLiveWorkFlowByTeacherIdQueryHandler : IRequestHandler<SearchClassLiveByTeacherIdQuery, MethodResult<PagingItemsModel<ClassLiveModel>>>
    {
        private readonly AuthContext _authContext;
        private readonly IClassLiveCalendarRepository _classLiveCalendarRepository;
        private readonly IClassRepository _classRepository;
        private readonly IUserService _userService;
        private readonly ICourseService _courseService;
        private readonly ISystemService _systemService;

        public SearchClassLiveWorkFlowByTeacherIdQueryHandler(AuthContext authContext,
            IClassLiveCalendarRepository classLiveCalendarRepository,
            IClassRepository classRepository,
            IUserService userService,
            ICourseService courseService,
            ISystemService systemService)
        {
            _authContext = authContext;
            _classLiveCalendarRepository = classLiveCalendarRepository;
            _classRepository = classRepository;
            _userService = userService;
            _courseService = courseService;
            _systemService = systemService;
        }

        public async Task<MethodResult<PagingItemsModel<ClassLiveModel>>> Handle(SearchClassLiveByTeacherIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<ClassLiveModel>>();
            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
            var teacherResult = await _userService.GetTeacherByUserIdAsync(_authContext.CurrentUserId);
            var teacherId = teacherResult.Content?.Result?.Id;
            var r1 = _classRepository.Queryable
                 .Where(x => x != null && x.TeacherId == teacherId)
                 .AsNoTracking()
                 .Select(x => new ClassLiveModel
                 {
                     Id = x.Id,
                     Code = x.Code,
                     CourseId = x.CourseId,
                     CreatedDate = x.CreatedDate,
                     StartDate = x.StartDate,
                     EndDate = x.EndDate,
                     LiveDays = x.LiveDays,
                     LiveTimeFrameId = x.LiveTimeFrameId
                 })
                 .AsEnumerable();

            var r2 = _classLiveCalendarRepository.Queryable
                .Where(x => x.Class != null && x.TeacherId == teacherId)
                .Include(x => x.Class)
                .AsNoTracking()
                .Select(x => new ClassLiveModel
                {
                    Id = x.Id,
                    Code = x.Class!.Code,
                    CourseId = x.Class!.CourseId,
                    CreatedDate = x.CreatedDate,
                    StartDate = x.LiveDate,
                    EndDate = x.LiveDate,
                    LiveDays = new List<DayOfWeek> { x.LiveDate.DayOfWeek },
                    LiveTimeFrameId = x.LiveTimeFrameId
                })
                .AsEnumerable();

            var query = r1.Union(r2);
            int totalItem = query.Count();
            var lists = query.Skip((request!.Page - 1) * request!.PageSize).Take(request!.PageSize).ToList();

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

            methodResult.Result = new PagingItemsModel<ClassLiveModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
