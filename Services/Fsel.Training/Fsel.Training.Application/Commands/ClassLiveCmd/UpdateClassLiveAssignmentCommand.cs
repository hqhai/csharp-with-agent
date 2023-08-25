// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Commands.ClassLiveCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Shared.Enums;
    using Fsel.Training.Application.Services.CourseServices;
    using Fsel.Training.Application.Services.SystemServices;
    using Fsel.Training.Application.Services.UserServices;
    using Fsel.Training.Domain.Entities;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.CommandModels.ClassLives;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateClassLiveAssignmentCommand : ApproveClassLiveCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class UpdateClassLiveAssignmentCommandHandler : IRequestHandler<UpdateClassLiveAssignmentCommand, MethodResult<bool>>
    {
        private readonly AuthContext _authContext;
        private readonly IClassRepository _classRepository;
        private readonly IClassLiveWorkFlowRepository _classLiveWorkFlowRepository;
        private readonly IUserService _userService;
        private readonly ICourseService _courseService;
        private readonly ISystemService _systemService;

        public UpdateClassLiveAssignmentCommandHandler(AuthContext authContext,
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

        public async Task<MethodResult<bool>> Handle(UpdateClassLiveAssignmentCommand request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<bool>();
            ArgumentNullException.ThrowIfNull(request);

            var timeFramesResult = await _systemService.GetLiveTimeFramesAsync();
            var timeFrames = timeFramesResult.Content?.Result;
            var teacherResult = await _userService.GetTeacherByUserIdAsync(_authContext.CurrentUserId);
            var teacherId = teacherResult.Content?.Result?.Id;
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

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = true;
            return methodResult;
        }
    }
}
