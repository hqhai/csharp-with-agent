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

    public class GetCalendarByStudentQuery : GetCalendarQueryModel, IRequest<MethodResult<IList<ClassLiveCalendarModel>>>
    {
    }

    public class GetCalendarByStudentQueryHandler : IRequestHandler<GetCalendarByStudentQuery, MethodResult<IList<ClassLiveCalendarModel>>>
    {
        private readonly IClassLiveCalendarRepository _classLiveCalendarRepository;
        private readonly ICourseService _courseService;
        private readonly ISystemService _systemService;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly AuthContext _authContext;

        public GetCalendarByStudentQueryHandler(IClassLiveCalendarRepository classLiveCalendarRepository, IUserService userService, IMapper mapper, AuthContext authContext, ICourseService courseService, ISystemService systemService)
        {
            _classLiveCalendarRepository = classLiveCalendarRepository;
            _userService = userService;
            _mapper = mapper;
            _authContext = authContext;
            _courseService = courseService;
            _systemService = systemService;
        }

        public async Task<MethodResult<IList<ClassLiveCalendarModel>>> Handle(GetCalendarByStudentQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var methodResult = new MethodResult<IList<ClassLiveCalendarModel>>();
            methodResult.Result = new List<ClassLiveCalendarModel>();

            if (!request.StartDate.HasValue || !request.EndDate.HasValue)
            {
                return methodResult;
            }

            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddError(studentResult.Error);
                return methodResult;
            }
            var student = studentResult.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }

            var query = _classLiveCalendarRepository.Queryable
                        .Include(x => x.Class)
                        .Where(x => x.LiveDate.Date >= request.StartDate.Value.Date &&
                                    x.LiveDate.Date <= request.EndDate.Value.Date)
                        .Where(x => x.ClassId == student.ClassId)
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
                            CourseId = x.Class!.CourseId,
                            ClassCode = x.Class!.Code,
                        });

            var lists = await query.OrderBy(x => x.LiveDate)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            var coursesReq = _courseService.GetCourseByIdsFromTeacherAsync(lists.Select(x => x.CourseId ?? default).Distinct().ToList());
            var teachersReq = _userService.GetTeacherByIdsAsync(new GetTeacherByIdsQueryModel { Ids = lists.Select(x => x.TeacherId ?? default).Distinct().ToList() });
            var liveTimeFramesReq = _systemService.GetLiveTimeFramesAsync();
            await Task.WhenAll(coursesReq, teachersReq, liveTimeFramesReq);

            var coursesResult = coursesReq.GetAwaiter().GetResult();
            var teachersResult = teachersReq.GetAwaiter().GetResult();
            var liveTimeFramesResult = liveTimeFramesReq.GetAwaiter().GetResult();

            var courses = coursesResult.Content?.Result;
            var teachers = teachersResult.Content?.Result;
            var liveTimeFrames = liveTimeFramesResult.Content?.Result;

            lists.ForEach(item =>
            {
                var teacher = teachers?.FirstOrDefault(x => x.Id == item.TeacherId);
                item.TeacherName = teacher?.User?.FullName;
                item.TeacherAvatar = teacher?.User?.AvatarPath;
                var course = courses?.FirstOrDefault(x => x.Id == item.CourseId);
                item.CourseLevel = course?.CourseLevel;

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
