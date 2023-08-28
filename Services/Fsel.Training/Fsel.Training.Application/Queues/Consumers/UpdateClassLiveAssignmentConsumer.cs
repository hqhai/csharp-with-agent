using Fsel.Core.Base.Interfaces;
using Fsel.Shared.Constants;
using Fsel.Shared.Enums;
using Fsel.Training.Application.Services.SystemServices;
using Fsel.Training.Domain.Entities;
using Fsel.Training.Domain.IRepositories;
using Fsel.Training.Domain.Models.EntityModels;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Training.Application.Queues.Consumers
{
    public class UpdateClassLiveAssignmentConsumer
    {
        private readonly IQueueProvider _queueProvider;
        private readonly IClassRepository _classRepository;
        private readonly IClassLiveWorkFlowRepository _classLiveWorkFlowRepository;
        private readonly IClassLiveCalendarRepository _classLiveCalendarRepository;
        private readonly ISystemService _systemService;

        public UpdateClassLiveAssignmentConsumer(
            IQueueProvider queueProvider,
            IClassRepository classRepository,
            IClassLiveWorkFlowRepository classLiveWorkFlowRepository,
            IClassLiveCalendarRepository classLiveCalendarRepository,
            ISystemService systemService)
        {
            _queueProvider = queueProvider;
            _classRepository = classRepository;
            _classLiveWorkFlowRepository = classLiveWorkFlowRepository;
            _classLiveCalendarRepository = classLiveCalendarRepository;
            _systemService = systemService;
        }

        public async Task Publish(CancellationToken cancellationToken)
        {
            var timeFramesResult = await _systemService.GetLiveTimeFramesAsync();
            if (!timeFramesResult.IsSuccessStatusCode)
            {
                return;
            }
            var timeFrames = timeFramesResult.Content?.Result;
            var classPendingIds = await _classRepository.Queryable.Where(x => x.TeacherApprovalStatus == EnumTeacherApprovalStatus.Pending).Select(x => x.Id).ToListAsync(cancellationToken);
            List<ClassLiveCalendar> classLiveCalendars = new List<ClassLiveCalendar>();
            foreach (var classId in classPendingIds)
            {
                var classLiveCalendar = await _classLiveCalendarRepository.Queryable.Where(x => classPendingIds.Contains(x.ClassId) && x.Status == EnumClassLiveCalendarStatus.NotStudied)
                    .OrderByDescending(x => x.LiveDate).FirstOrDefaultAsync(cancellationToken);
                if (classLiveCalendar != null)
                {
                    classLiveCalendars.Add(classLiveCalendar);
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
                    LiveTimeFrameId = x.ClassLiveCalendar.LiveTimeFrameId
                }).ToListAsync(cancellationToken);
            var query = r1;
            if (classLiveCalendars.Count > 0)
            {
                var r2 = classLiveCalendars
                .Select(x => new ClassLiveModel
                {
                    Id = x.Id,
                    StartDate = x.LiveDate,
                    LiveTimeFrameId = x.LiveTimeFrameId
                }).ToList();
                query = query.Union(r2).ToList();
            }
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
                        if (x.ClassLiveCalendar != null)
                        {
                            x.ClassLiveCalendar.TeacherId = x.TeacherId;
                        }
                    });
                    _classLiveWorkFlowRepository.UpdateList(classLiveWordFlows);
                    await _classLiveWorkFlowRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                }
            }
            await _queueProvider.Publish<object>(QueueSettings.RealtimeQueue.NameQueue.UpdateClassLiveAssignment, null, cancellationToken);
        }
    }
}
