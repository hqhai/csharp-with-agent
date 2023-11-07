// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Commands.ClassLiveCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Enums;
    using Fsel.Training.Domain.Entities;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.EntityModels;
    using Fsel.Training.Infrastructure.Repositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateClassLiveAssignmentCommand : IRequest<MethodResult<bool>>
    {
    }

    public class UpdateClassLiveAssignmentCommandHandler : IRequestHandler<UpdateClassLiveAssignmentCommand, MethodResult<bool>>
    {
        private readonly IClassRepository _classRepository;
        private readonly IClassLiveWorkFlowRepository _classLiveWorkFlowRepository;
        private readonly IClassLiveCalendarRepository _classLiveCalendarRepository;
        private readonly ITeacherFreeDateRepository _teacherFreeDateRepository;

        public UpdateClassLiveAssignmentCommandHandler(
            IClassRepository classRepository,
            IClassLiveWorkFlowRepository classLiveWorkFlowRepository,
            IClassLiveCalendarRepository classLiveCalendarRepository,
            ITeacherFreeDateRepository teacherFreeDateRepository)
        {
            _classRepository = classRepository;
            _classLiveWorkFlowRepository = classLiveWorkFlowRepository;
            _classLiveCalendarRepository = classLiveCalendarRepository;
            _teacherFreeDateRepository = teacherFreeDateRepository;
        }

        public async Task<MethodResult<bool>> Handle(UpdateClassLiveAssignmentCommand request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<bool>();
            ArgumentNullException.ThrowIfNull(request);

            var classPendingTeachers = await _classRepository.Queryable.Where(x => x.TeacherApprovalStatus == EnumTeacherApprovalStatus.Pending && x.TeacherId != null).ToListAsync(cancellationToken);
            List<ClassLiveCalendar> classLiveCalendars = new List<ClassLiveCalendar>();
            List<Class> classPendings = new List<Class>();
            foreach (var classPending in classPendingTeachers)
            {
                var classLiveCalendar = await _classLiveCalendarRepository.Queryable.Where(x => x.ClassId == classPending.Id && x.Status == EnumClassLiveCalendarStatus.NotStudied)
                    .OrderByDescending(x => x.LiveDate).FirstOrDefaultAsync(cancellationToken);
                if (classLiveCalendar != null)
                {
                    classLiveCalendars.Add(classLiveCalendar);
                }
                else
                {
                    classPendings.Add(classPending);
                }
            }

            var r1 = await _classLiveWorkFlowRepository.Queryable
                .Include(x => x.ClassLiveCalendar)
                .ThenInclude(x => x!.Class)
                .Where(x => x.Type == EnumWorkFlowType.AssignTeacher && x.Status == EnumWorkFlowAssignTeacherStatus.Pending.ToString())
                .Select(x => new ClassLiveModel
                {
                    Id = x.Id,
                    StartDate = x.ClassLiveCalendar!.LiveDate,
                }).ToListAsync(cancellationToken);
            var query = r1;
            if (classLiveCalendars.Count > 0)
            {
                var r2 = classLiveCalendars
                .Select(x => new ClassLiveModel
                {
                    Id = x.ClassId,
                    StartDate = x.LiveDate,
                }).ToList();
                query = query.Union(r2).ToList();
            }

            if (classPendings.Count > 0)
            {
                var r2 = classPendings
                .Select(x => new ClassLiveModel
                {
                    Id = x.Id,
                    StartDate = x.StartDate,
                }).ToList();
                query = query.Union(r2).ToList();
            }
            IList<Guid> ids = new List<Guid>();
            foreach (var item in query)
            {
                if (item.StartDate.HasValue)
                {
                    if (item.StartDate.Value.Date == DateTime.UtcNow.Date.AddDays(1))
                    {
                        ids.Add(item.Id);
                    }
                }
            }
            if (ids.Count > 0)
            {
                var classLiveWordFlows = await _classLiveWorkFlowRepository.Queryable.Include(x => x.ClassLiveCalendar).Where(x => ids.Contains(x.Id)).ToListAsync(cancellationToken);
                var classes = await _classRepository.Queryable.Include(x => x.ClassLiveCalendars).Where(x => ids.Contains(x.Id)).ToListAsync(cancellationToken);
                var listTeacherFreeDates = new List<TeacherFreeDate>();
                if (classes.Count > 0)
                {
                    classes.ForEach(x =>
                    {
                        if (x.Status == EnumClassStatus.Active)
                        {
                            var classLiveCalendars = x.ClassLiveCalendars.Where(classLive => classLive.LiveDate.Date > DateTime.UtcNow)
                                         .Select(classLive =>
                                         {
                                             classLive.TeacherId = x.TeacherId;
                                             return classLive;
                                         }).ToList();

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

                    foreach (var @class in classes)
                    {
                        var teacherFreeDates = await _teacherFreeDateRepository.Queryable
                                   .Where(x => x.TeacherId == @class.TeacherId).Include(x => x.TeacherFreeTimes).ThenInclude(tfl => tfl.TeacherFreeTimeLives)
                                   .ToListAsync(cancellationToken);

                        foreach (var item in teacherFreeDates.SelectMany(p => p.TeacherFreeTimes).SelectMany(x => x.TeacherFreeTimeLives))
                        {
                            if (@class.ClassLiveCalendars != null && @class.ClassLiveCalendars.Where(m => m.Status == EnumClassLiveCalendarStatus.NotStudied).Any(p => p.LiveTimeFrameId == item.TeacherFreeTime?.LiveTimeFrameId && p.LiveDate.Date == item.LiveDate.Date))
                            {
                                item.IsUsed = true;
                            }
                        }
                        listTeacherFreeDates.AddRange(teacherFreeDates);
                    }
                }
                if (classLiveWordFlows.Count > 0)
                {
                    classLiveWordFlows.ForEach(x =>
                    {
                        x.Status = EnumWorkFlowAssignTeacherStatus.Approved.ToString();
                        if (x.ClassLiveCalendar != null)
                        {
                            x.ClassLiveCalendar.TeacherId = x.TeacherId;
                        }
                    });
                    _classLiveWorkFlowRepository.UpdateList(classLiveWordFlows);
                    await _classLiveWorkFlowRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                    foreach (var item in classLiveWordFlows)
                    {
                        var teacherFreeDates = await _teacherFreeDateRepository.Queryable.Where(n => n.TeacherId == item.TeacherId).Include(p => p.TeacherFreeTimes).ThenInclude(x => x.TeacherFreeTimeLives).ToListAsync(cancellationToken);

                        foreach (var tf in teacherFreeDates.SelectMany(p => p.TeacherFreeTimes).SelectMany(tf => tf.TeacherFreeTimeLives.Where(tfl => tfl.LiveDate == item.ClassLiveCalendar?.LiveDate && tfl.TeacherFreeTime?.LiveTimeFrameId == item.ClassLiveCalendar.LiveTimeFrameId)))
                        {
                            tf.IsUsed = true;
                        }

                        listTeacherFreeDates.AddRange(teacherFreeDates);
                    }
                }
                _teacherFreeDateRepository.UpdateList(listTeacherFreeDates);
                await _teacherFreeDateRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = true;
            return methodResult;
        }
    }
}
