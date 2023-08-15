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
    using Fsel.Training.Domain.Entities;
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
        private readonly IClassRepository _classRepository;
        private readonly IClassLiveWorkFlowRepository _classLiveWorkFlowRepository;
        private readonly IUserService _userService;
        private readonly ICourseService _courseService;
        private readonly ISystemService _systemService;

        public SearchClassLiveAssignmentsQueryHandler(AuthContext authContext,
            IClassRepository classRepository,
            IClassLiveWorkFlowRepository classLiveWorkFlowRepository,
            IUserService userService,
            ICourseService courseService,
            ISystemService systemService)
        {
            _authContext = authContext;
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

            var r1 = _classRepository.Queryable.Include(x => x.ClassLiveCalendars).Where(x => x.LiveTimeFrameId != null && x.TeacherId == teacherId && x.TeacherApprovalStatus == EnumTeacherApprovalStatus.Pending)
                 .Select(x => new ClassLiveModel
                 {
                     Id = x.Id,
                     StartDate = x.StartDate,
                     LiveTimeFrameId = x.LiveTimeFrameId
                 })
                 .ToList();

            var r2 = _classLiveWorkFlowRepository.Queryable
                .Include(x => x.ClassLiveCalendar)
                .ThenInclude(x => x!.Class)
                .Where(x => x.Type == EnumWorkFlowType.AssignTeacher && x.Status == EnumWorkFlowAssignTeacherStatus.Pending.ToString() && x.TeacherId == teacherId)
                .Select(x => new ClassLiveModel
                {
                    Id = x.Id,
                    StartDate = x.ClassLiveCalendar!.LiveDate,
                    LiveTimeFrameId = x.ClassLiveCalendar.LiveTimeFrameId
                })
                .ToList();
            var query = r1.Union(r2);
            IList<Guid> ids = new List<Guid>();
            foreach (var item in query)
            {
                var liveTimeFrame = timeFrames?.FirstOrDefault(x => x.Id == item.LiveTimeFrameId);
                item.StartTime = liveTimeFrame?.StartTime ?? default;
                if (item.StartDate != null)
                {
                    DateTime dateTime = DateTime.Now;
                    double hours = item.StartTime;
                    var assignTeacher = item.StartDate.Value.Date.AddHours(hours);
                    item.IsStatus = assignTeacher < dateTime.AddDays(1);
                    if (item.IsStatus)
                    {
                        ids.Add(item.Id);
                    }
                }
            }

            #endregion update

            if (ids.Count > 0)
            {
                var classLiveWordFlows = await _classLiveWorkFlowRepository.Queryable.Include(x => x.ClassLiveCalendar).Where(x => ids.Contains(x.Id)).ToListAsync(cancellationToken);
                var classes = await _classRepository.Queryable.Include(x => x.ClassLiveCalendars).Where(x => ids.Contains(x.Id)).ToListAsync(cancellationToken);
                if (classes.Count > 0)
                {
                    classes.ForEach(x =>
                    {
                        if (x.Status == EnumStatusClass.Active)
                        {
                            var classLiveCalendars = x.ClassLiveCalendars
                                         .Where(classLive => timeFrames?.FirstOrDefault(x => x.Id == classLive.LiveTimeFrameId)?.StartTime != null &&
                                             classLive.LiveDate.Date.AddHours(timeFrames.First(x => x.Id == classLive.LiveTimeFrameId)?.StartTime ?? default) > DateTime.Now
                                         )
                                         .Select(classLive => new ClassLiveCalendar
                                         {
                                             TeacherId = x.TeacherId,
                                         })
                                         .ToList();
                            x.ClassLiveCalendars = classLiveCalendars;
                            x.TeacherApprovalStatus = EnumTeacherApprovalStatus.Approved;
                        }
                        else
                        {
                            x.TeacherApprovalStatus = EnumTeacherApprovalStatus.Approved;
                        }
                    });
                    _classRepository.UpdateList(classes);
                    await _classRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                }
                if (classLiveWordFlows.Count > 0)
                {
                    classLiveWordFlows.ForEach(x =>
                    {
                        x.Status = EnumWorkFlowAssignTeacherStatus.Approved.ToString();
                        x.ClassLiveCalendar!.TeacherId = teacherId;
                    });
                    _classLiveWorkFlowRepository.UpdateList(classLiveWordFlows);
                    await _classLiveWorkFlowRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                }
            }

            #region Search

            var a1 = _classRepository.Queryable
                 .Where(x => x.LiveTimeFrameId != null && x.TeacherId == teacherId && x.Status == EnumStatusClass.New && x.TeacherApprovalStatus == EnumTeacherApprovalStatus.Pending)
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

            var a2 = _classLiveWorkFlowRepository.Queryable
                .Include(x => x.ClassLiveCalendar)
                .ThenInclude(x => x!.Class)
                .Where(x => x.Type == EnumWorkFlowType.AssignTeacher && x.Status == EnumWorkFlowAssignTeacherStatus.Pending.ToString() && x.TeacherId == teacherId)
                .AsNoTracking()
                .Select(x => new ClassLiveModel
                {
                    Id = x.Id,
                    Code = x.ClassLiveCalendar!.Class!.Code,
                    Status = x.Status,
                    CourseId = x.ClassLiveCalendar.Class!.CourseId,
                    CreatedDate = x.CreatedDate,
                    StartDate = x.ClassLiveCalendar.LiveDate,
                    EndDate = x.ClassLiveCalendar.LiveDate,
                    LiveDays = new List<DayOfWeek> { x.ClassLiveCalendar.LiveDate.DayOfWeek },
                    LiveTimeFrameId = x.ClassLiveCalendar.LiveTimeFrameId
                })
                .AsEnumerable();
            query = a1.Union(a2);
            int totalItem = query.Count();
            var lists = query.Skip((request!.Page - 1) * request!.PageSize).Take(request!.PageSize).ToList();

            #endregion Search

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
                    item.IsStatus = item.StartDate!.Value.Date.AddDays(-1) > DateTime.Now.Date;
                }
            }

            methodResult.Result = new PagingItemsModel<ClassLiveModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
