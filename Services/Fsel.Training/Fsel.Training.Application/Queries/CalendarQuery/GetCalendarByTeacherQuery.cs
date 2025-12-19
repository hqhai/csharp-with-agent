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
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.EntityModels;
    using Fsel.Training.Domain.Models.QueryModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetCalendarByTeacherQuery : GetCalendarByTeacherQueryModel, IRequest<MethodResult<IList<ClassLiveCalendarModel>>>
    {
    }

    public class GetCalendarByTeacherQueryHandler : IRequestHandler<GetCalendarByTeacherQuery, MethodResult<IList<ClassLiveCalendarModel>>>
    {
        private readonly IClassLiveCalendarRepository _classLiveCalendarRepository;
        private readonly ICourseService _courseService;
        private readonly ISystemService _systemService;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly AuthContext _authContext;

        public GetCalendarByTeacherQueryHandler(IClassLiveCalendarRepository classLiveCalendarRepository, IUserService userService, IMapper mapper, AuthContext authContext, ICourseService courseService, ISystemService systemService)
        {
            _classLiveCalendarRepository = classLiveCalendarRepository;
            _userService = userService;
            _mapper = mapper;
            _authContext = authContext;
            _courseService = courseService;
            _systemService = systemService;
        }

        public async Task<MethodResult<IList<ClassLiveCalendarModel>>> Handle(GetCalendarByTeacherQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var methodResult = new MethodResult<IList<ClassLiveCalendarModel>>();
            methodResult.Result = new List<ClassLiveCalendarModel>();

            if (!request.StartDate.HasValue || !request.EndDate.HasValue)
            {
                return methodResult;
            }

            var teacherReq = _userService.GetTeacherByUserIdAsync(_authContext.CurrentUserId);
            var coursesReq = _courseService.GetCoursesByLevelFromTeacherAsync(request.Level);

            await Task.WhenAll(teacherReq, coursesReq);
            var teacherResult = teacherReq.GetAwaiter().GetResult();
            var coursesResult = coursesReq.GetAwaiter().GetResult();

            if (!teacherResult.IsSuccessStatusCode || !coursesResult.IsSuccessStatusCode)
            {
                methodResult.AddError(teacherResult.Error);
                methodResult.AddError(coursesResult.Error);
                return methodResult;
            }
            var teacher = teacherResult.Content?.Result;
            if (teacher == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(teacher));
                return methodResult;
            }

            var courses = coursesResult.Content?.Result;

            var query = _classLiveCalendarRepository.Queryable
                        .Include(x => x.Class)
                        .Where(x => x.LiveDate.Date >= request.StartDate.Value.Date &&
                                    x.LiveDate.Date <= request.EndDate.Value.Date)
                        .Where(x => x.TeacherId == teacher.Id)
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
                            Class = _mapper.Map<ClassModel>(x.Class)
                        });

            if (request.Level.HasValue)
            {
                query = query.Where(m => courses != null && courses.Select(x => x.Id).Contains(m.Class!.CourseId));
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
            await Task.WhenAll(teacherReq, liveTimeFramesReq);

            coursesResult = coursesReq.GetAwaiter().GetResult();
            var liveTimeFramesResult = liveTimeFramesReq.GetAwaiter().GetResult();

            courses = coursesResult.Content?.Result;
            var liveTimeFrames = liveTimeFramesResult.Content?.Result;

            lists.ForEach(item =>
            {
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
