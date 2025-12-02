// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.ClassLiveWorkFlowQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Shared.Enums;
    using Fsel.Training.Application.Services.CourseServices;
    using Fsel.Training.Application.Services.SystemServices;
    using Fsel.Training.Application.Services.UserServices;
    using Fsel.Training.Application.Services.UserServices.Models;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.EntityModels;
    using Fsel.Training.Domain.Models.QueryModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchClassLiveWorkFlowQuery : SearchClassLiveWorkFlowQueryModel, IRequest<MethodResult<PagingItemsModel<SearchClassLiveWorkFlowModel>>>
    {
    }

    public class SearchSearchClassLiveWorkFlowQueryHandler : IRequestHandler<SearchClassLiveWorkFlowQuery, MethodResult<PagingItemsModel<SearchClassLiveWorkFlowModel>>>
    {
        private readonly IClassLiveWorkFlowRepository _classLiveWorkFlowRepository;
        private readonly ISystemService _systemService;
        private readonly IUserService _userService;
        private readonly ICourseService _courseService;
        private readonly AuthContext _authContext;

        public SearchSearchClassLiveWorkFlowQueryHandler(IClassLiveWorkFlowRepository classLiveWorkFlowRepository, ISystemService systemService, IUserService userService, ICourseService courseService, AuthContext authContext)
        {
            _classLiveWorkFlowRepository = classLiveWorkFlowRepository;
            _systemService = systemService;
            _userService = userService;
            _courseService = courseService;
            _authContext = authContext;
        }

        public async Task<MethodResult<PagingItemsModel<SearchClassLiveWorkFlowModel>>> Handle(SearchClassLiveWorkFlowQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<SearchClassLiveWorkFlowModel>>();
            var teacherInfo = await _userService.GetTeacherByUserIdAsync(_authContext.CurrentUserId);
            var teacherId = teacherInfo.Content?.Result?.Id;
            var classLiveWorkFlowQuery = _classLiveWorkFlowRepository.Queryable
                                                .Include(x => x.ClassLiveCalendar)
                                                .Where(x => x.ClassLiveCalendar != null && x.TeacherId == teacherId && x.Type == EnumWorkFlowType.ChangeTeacher)
                                                .AsNoTracking()
                                                .Select(x => new SearchClassLiveWorkFlowModel
                                                {
                                                    Id = x.Id,
                                                    CreatedDate = x.CreatedDate,
                                                    Type = x.Type,
                                                    Status = x.Status,
                                                    LiveTimeFrameId = x.ClassLiveCalendar!.LiveTimeFrameId,
                                                    ClassName = x.ClassLiveCalendar.Class!.Name,
                                                    ClassCode = x.ClassLiveCalendar.Class.Code,
                                                    LiveDate = x.ClassLiveCalendar.LiveDate,
                                                    CourseId = x.ClassLiveCalendar.Class.CourseId,
                                                    TeacherId = x.ClassLiveCalendar.TeacherId
                                                });
            int totalItem = await classLiveWorkFlowQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await classLiveWorkFlowQuery
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            var courseIds = lists.Select(x => x.CourseId ?? default).Distinct().ToList();
            var courseResults = await _courseService.GetListCourseByIds(courseIds);
            var courses = courseResults.Content?.Result;
            var teachersResult = await _userService.GetTeacherByIdsAsync(new GetTeacherByIdsQueryModel { Ids = lists.Select(x => x.TeacherId ?? default).ToList() });
            var teachers = teachersResult.Content?.Result;
            var liveTimeFramesResult = await _systemService.GetLiveTimeFramesAsync();
            var liveTimeFrames = liveTimeFramesResult.Content?.Result;

            foreach (var item in lists)
            {
                var course = courses?.FirstOrDefault(x => x.Id == item.CourseId);
                item.CourseLevel = course?.CourseLevel;

                var liveTimeFrame = liveTimeFrames?.FirstOrDefault(x => x.Id == item.LiveTimeFrameId);
                if (liveTimeFrame != null)
                {
                    item.StartTime = liveTimeFrame.StartTime;
                    item.EndTime = liveTimeFrame.EndTime;
                }
                var teacher = teachers?.FirstOrDefault(x => x.Id == item.TeacherId);
                item.TeacherName = teacher?.User?.FullName;
            }
            methodResult.Result = new PagingItemsModel<SearchClassLiveWorkFlowModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
