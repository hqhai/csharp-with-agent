// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.CalendarQuery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
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

    public class GetCalendarByCsoQuery : GetCalendarByCsoQueryModel, IRequest<MethodResult<IList<ClassLiveCalendarModel>>>
    {
    }

    public class GetCalendarByCsoQueryHandler : IRequestHandler<GetCalendarByCsoQuery, MethodResult<IList<ClassLiveCalendarModel>>>
    {
        private readonly IClassLiveCalendarRepository _classLiveCalendarRepository;
        private readonly ICourseService _courseService;
        private readonly ISystemService _systemService;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly AuthContext _authContext;

        public GetCalendarByCsoQueryHandler(IClassLiveCalendarRepository classLiveCalendarRepository, IUserService userService, IMapper mapper, AuthContext authContext, ICourseService courseService, ISystemService systemService)
        {
            _classLiveCalendarRepository = classLiveCalendarRepository;
            _userService = userService;
            _mapper = mapper;
            _authContext = authContext;
            _courseService = courseService;
            _systemService = systemService;
        }

        public async Task<MethodResult<IList<ClassLiveCalendarModel>>> Handle(GetCalendarByCsoQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var methodResult = new MethodResult<IList<ClassLiveCalendarModel>>();
            methodResult.Result = new List<ClassLiveCalendarModel>();

            if (!request.StartDate.HasValue || !request.EndDate.HasValue)
            {
                return methodResult;
            }

            var csoReq = _userService.GetCsoByUserIdAsync(_authContext.CurrentUserId);
            var coursesReq = _courseService.GetCoursesByLevelAsync(request.Level);

            await Task.WhenAll(csoReq, coursesReq);
            var csoResult = csoReq.GetAwaiter().GetResult();
            var coursesResult = coursesReq.GetAwaiter().GetResult();

            if (!csoResult.IsSuccessStatusCode || !coursesResult.IsSuccessStatusCode)
            {
                methodResult.AddError(csoResult.Error);
                methodResult.AddError(coursesResult.Error);
                return methodResult;
            }
            var cso = csoResult.Content?.Result;
            if (cso == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(cso));
                return methodResult;
            }
            var courses = coursesResult.Content?.Result;

            var query = _classLiveCalendarRepository.Queryable
                        .Include(x => x.Class)
                        .Where(x => x.LiveDate.Date >= request.StartDate.Value.Date &&
                                    x.LiveDate.Date <= request.EndDate.Value.Date)
                        .Where(x => x.Class!.CsoId == cso.Id)
                        .Select(x => new ClassLiveCalendarModel
                        {
                            Id = x.Id,
                            CreatedFullName = x.CreatedFullName,
                            CreatedDate = x.CreatedDate,
                            CreatedUserId = x.CreatedUserId,
                            LiveDate = x.LiveDate,
                            LiveTimeFrameId = x.LiveTimeFrameId,
                            AccessLink = x.AccessLink,
                            Note = x.Note,
                            Status = x.Status,
                            ClassId = x.ClassId,
                            ClassCode = x.Class!.Code,
                            TeacherId = x.TeacherId,
                            Class = _mapper.Map<ClassModel>(x.Class)
                        });

            if (request.Level.HasValue)
            {
                query = query.Where(m => courses != null && courses.Select(x => x.Id).Contains(m.Class!.CourseId));
            }

            if (request.TeacherId.HasValue)
            {
                query = query.Where(m => m.TeacherId == request.TeacherId.Value);
            }

            if (request.ClassId.HasValue)
            {
                query = query.Where(m => m.ClassId == request.ClassId.Value);
            }

            var lists = await query.OrderBy(x => x.LiveDate)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            coursesReq = _courseService.GetCourseByIdsFromTeacherAsync(lists.Select(x => x.Class!.CourseId).ToList());
            var liveTimeFramesReq = _systemService.GetLiveTimeFramesAsync();
            var teachersReq = _userService.GetTeacherByIdsAsync(new GetTeacherByIdsQueryModel { Ids = lists.Select(x => x.TeacherId ?? default).ToList() });

            await Task.WhenAll(teachersReq, liveTimeFramesReq, coursesReq);

            coursesResult = coursesReq.GetAwaiter().GetResult();
            var liveTimeFramesResult = liveTimeFramesReq.GetAwaiter().GetResult();
            var teachersResult = teachersReq.GetAwaiter().GetResult();

            courses = coursesResult.Content?.Result;
            var liveTimeFrames = liveTimeFramesResult.Content?.Result;
            var teachers = teachersResult.Content?.Result;

            lists.ForEach(item =>
            {
                var teacher = teachers?.FirstOrDefault(x => x.Id == item.TeacherId);
                item.TeacherName = teacher?.User?.FullName;
                item.TeacherAvatar = teacher?.User?.AvatarPath;

                var course = courses?.FirstOrDefault(x => x.Id == item.Class?.CourseId);
                item.Class!.CourseLevel = course?.CourseLevel;

                var liveTimeFrame = liveTimeFrames?.FirstOrDefault(x => x.Id == item.LiveTimeFrameId);
                if (liveTimeFrame != null)
                {
                    item.StartTime = liveTimeFrame.StartTime;
                    item.EndTime = liveTimeFrame.EndTime;
                }
            });

            methodResult.Result = lists;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
