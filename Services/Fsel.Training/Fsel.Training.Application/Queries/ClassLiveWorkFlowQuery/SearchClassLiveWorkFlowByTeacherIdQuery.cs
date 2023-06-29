// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.ClassLiveWorkFlowQuery
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
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
    using Fsel.Training.Domain.Models.QueryModels.ClassLiveWorkFlowQuery;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchClassLiveWorkFlowByTeacherIdQuery : SearchClassLiveWorkFlowByTeacherIdQueryModel, IRequest<MethodResult<PagingItemsModel<ClassLiveWorkFlowSearchModel>>>
    {
    }

    public class SearchClassLiveWorkFlowByTeacherIdQueryHandler : IRequestHandler<SearchClassLiveWorkFlowByTeacherIdQuery, MethodResult<PagingItemsModel<ClassLiveWorkFlowSearchModel>>>
    {
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly ICourseService _courseService;
        private readonly IClassLiveWorkFlowRepository _classLiveWorkFlowRepository;
        private readonly ISystemService _systemService;

        public SearchClassLiveWorkFlowByTeacherIdQueryHandler(AuthContext authContext,
            IUserService userService,
            ICourseService courseService,
            IClassLiveWorkFlowRepository classLiveWorkFlowRepository,
            ISystemService systemService)
        {
            _authContext = authContext;
            _userService = userService;
            _courseService = courseService;
            _classLiveWorkFlowRepository = classLiveWorkFlowRepository;
            _systemService = systemService;
        }

        public async Task<MethodResult<PagingItemsModel<ClassLiveWorkFlowSearchModel>>> Handle(SearchClassLiveWorkFlowByTeacherIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<ClassLiveWorkFlowSearchModel>>();
            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
            var teacherResult = await _userService.GetTeacherByUserIdAsync(_authContext.CurrentUserId);
            var teacher = teacherResult.Content?.Result;
            var query = _classLiveWorkFlowRepository.Queryable
                                    .Include(x => x.ClassLiveCalendar)
                                    .ThenInclude(x => x!.Class)
                                    .Where(x => x.TeacherId == teacher!.Id)
                                    .Select(x => GetByWorkFlow(x));

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
            methodResult.Result = new PagingItemsModel<ClassLiveWorkFlowSearchModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        public ClassLiveWorkFlowSearchModel GetByWorkFlow(ClassLiveWorkFlow classLiveWorkFlow)
        {
            ArgumentNullException.ThrowIfNull(classLiveWorkFlow);
            var classLiveCalendar = classLiveWorkFlow.ClassLiveCalendar;
            var @class = classLiveCalendar?.Class;
            return new ClassLiveWorkFlowSearchModel
            {
                Id = classLiveWorkFlow.Id,
                CreatedDate = classLiveWorkFlow.CreatedDate,
                CourseId = @class?.CourseId ?? default,
                Code = @class?.Code,
                AccessLink = classLiveCalendar?.AccessLink,
                LiveTimeFrameId = @class?.LiveTimeFrameId ?? default,
                LiveDate = classLiveCalendar?.LiveDate ?? default
            };
        }
    }
}
