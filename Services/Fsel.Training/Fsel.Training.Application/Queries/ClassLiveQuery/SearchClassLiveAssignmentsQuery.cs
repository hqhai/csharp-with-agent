// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.ClassLiveQuery
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;
    using Fsel.Training.Application.Services.CourseServices;
    using Fsel.Training.Application.Services.SystemServices;
    using Fsel.Training.Application.Services.UserServices;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchClassLiveAssignmentsQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<ClassLiveModel>>>
    {
    }

    public class SearchClassLiveAssignmentsQueryHandler : IRequestHandler<SearchClassLiveAssignmentsQuery, MethodResult<PagingItemsModel<ClassLiveModel>>>
    {
        private readonly AuthContext _authContext;
        private readonly IClassLiveCalendarRepository _classLiveCalendarRepository;
        private readonly IClassRepository _classRepository;
        private readonly IClassLiveWorkFlowRepository _classLiveWorkFlowRepository;
        private readonly IUserService _userService;
        private readonly ICourseService _courseService;
        private readonly ISystemService _systemService;

        public SearchClassLiveAssignmentsQueryHandler(AuthContext authContext,
            IClassLiveCalendarRepository classLiveCalendarRepository,
            IClassRepository classRepository,
            IClassLiveWorkFlowRepository classLiveWorkFlowRepository,
            IUserService userService,
            ICourseService courseService,
            ISystemService systemService)
        {
            _authContext = authContext;
            _classLiveCalendarRepository = classLiveCalendarRepository;
            _classRepository = classRepository;
            _classLiveWorkFlowRepository = classLiveWorkFlowRepository;
            _userService = userService;
            _courseService = courseService;
            _systemService = systemService;
        }

        public async Task<MethodResult<PagingItemsModel<ClassLiveModel>>> Handle(SearchClassLiveAssignmentsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<ClassLiveModel>>();
            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
            var timeFramesResult = await _systemService.GetLiveTimeFramesAsync();
            var timeFrames = timeFramesResult.Content?.Result;
            var teacherResult = await _userService.GetTeacherByUserIdAsync(_authContext.CurrentUserId);
            var teacherId = teacherResult.Content?.Result?.Id;
            #region update
            var r1 = _classRepository.Queryable.Where(x => x != null && x.TeacherId == teacherId && x.TeacherApprovalStatus == EnumTeacherApprovalStatus.Pending)
                 .AsNoTracking()
                 .Select(x => new ClassLiveModel
                 {
                     Id = x.Id,
                     StartDate = x.StartDate,
                     LiveTimeFrameId = x.LiveTimeFrameId
                 })
                 .AsEnumerable();

            var r2 = _classLiveCalendarRepository.Queryable
                .Where(x => x.Class != null && x.ClassLiveWorkFlows.Any(x => x.Type == EnumWorkFlowType.AssignTeacher && x.Status == EnumWorkFlowAssignTeacherStatus.Pending.ToString() && x.TeacherId == teacherId))
                .AsNoTracking()
                .Select(x => new ClassLiveModel
                {
                    Id = x.Id,
                    StartDate = x.LiveDate,
                    LiveTimeFrameId = x.LiveTimeFrameId
                })
                .AsEnumerable();
            var query = r1.Union(r2);
            IList<Guid> ids = new List<Guid>();
            foreach (var item in query.ToList())
            {
                var liveTimeFrame = timeFrames?.FirstOrDefault(x => x.Id == item.LiveTimeFrameId);
                item.StartTime = liveTimeFrame?.StartTime ?? default;
                if (item.StartDate != null)
                {
                    DateTime dateTime = DateTime.Now;
                    double totalHours = item.StartTime % 24;
                    int hours = (int)totalHours;
                    var assignTeacher = item.StartDate.Value.Date.AddHours(hours);
                    item.IsStatus = assignTeacher < dateTime.AddDays(1);
                    if (item.IsStatus)
                    {
                        ids.Add(item.Id);
                    }
                }
            }
            #endregion

            if (ids.Count > 0)
            {
                var classLiveWordFlows = await _classLiveWorkFlowRepository.Queryable.Where(x => ids.Contains(x.ClassLiveCalendarId)).ToListAsync(cancellationToken);
                var classes = await _classRepository.Queryable.Where(x => ids.Contains(x.Id)).ToListAsync(cancellationToken);
                if (classes.Count > 0)
                {
                    classes.ForEach(x => x.TeacherApprovalStatus = EnumTeacherApprovalStatus.Approved);
                    _classRepository.UpdateList(classes);
                    await _classRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                }
                if (classLiveWordFlows.Count > 0)
                {
                    classLiveWordFlows.ForEach(x => x.Status = EnumWorkFlowAssignTeacherStatus.Approved.ToString());
                    _classLiveWorkFlowRepository.UpdateList(classLiveWordFlows);
                    await _classLiveWorkFlowRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                }
            }

            var a1 = _classRepository.Queryable
                 .Where(x => x != null && x.TeacherId == teacherId && x.TeacherApprovalStatus == EnumTeacherApprovalStatus.Pending)
                 .AsNoTracking()
                 .Select(x => new ClassLiveModel
                 {
                     Id = x.Id,
                     Code = x.Code,
                     Status = x.TeacherApprovalStatus.ToString(),
                     CourseId = x.CourseId,
                     CreatedDate = x.CreatedDate,
                     StartDate = x.StartDate,
                     EndDate = x.EndDate,
                     LiveDays = x.LiveDays,
                     LiveTimeFrameId = x.LiveTimeFrameId
                 })
                 .AsEnumerable();

            var a2 = _classLiveCalendarRepository.Queryable
                .Include(x => x.Class)
                .Include(x => x.ClassLiveWorkFlows)
                .Where(x => x.Class != null && x.ClassLiveWorkFlows.Any(x => x.Type == EnumWorkFlowType.AssignTeacher && x.Status == EnumWorkFlowAssignTeacherStatus.Pending.ToString() && x.TeacherId == teacherId))
                .AsNoTracking()
                .Select(x => new ClassLiveModel
                {
                    Id = x.Id,
                    Code = x.Class!.Code,
                    Status = x.ClassLiveWorkFlows.FirstOrDefault(x => x.Type == EnumWorkFlowType.AssignTeacher && x.Status == EnumWorkFlowAssignTeacherStatus.Pending.ToString() && x.TeacherId == teacherId)!.Status,
                    CourseId = x.Class!.CourseId,
                    CreatedDate = x.CreatedDate,
                    StartDate = x.LiveDate,
                    EndDate = x.LiveDate,
                    LiveDays = new List<DayOfWeek> { x.LiveDate.DayOfWeek },
                    LiveTimeFrameId = x.LiveTimeFrameId
                })
                .AsEnumerable();
            query = a1.Union(a2);
            int totalItem = query.Count();
            var lists = query.Skip((request!.Page - 1) * request!.PageSize).Take(request!.PageSize).ToList();

            var courseIds = lists.Select(x => x.CourseId).ToList();
            var courseResults = await _courseService.GetListCourseByIds(courseIds);
            var courses = courseResults.Content?.Result;

            foreach (var item in lists)
            {
                if (item != null)
                {
                    var liveTimeFrame = timeFrames?.FirstOrDefault(x => x.Id == item.LiveTimeFrameId);
                    item.CourseLevel = courses?.FirstOrDefault(x => x.Id == item.CourseId)?.CourseLevel ?? default;
                    item.StartTime = liveTimeFrame?.StartTime ?? default;
                    item.EndTime = liveTimeFrame?.EndTime ?? default;
                }
            }

            methodResult.Result = new PagingItemsModel<ClassLiveModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
