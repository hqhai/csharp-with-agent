using Fsel.Core.Base.BaseModels;
using Fsel.Shared.Enums;
using Fsel.Training.Domain.Entities;
using Fsel.Training.Domain.IRepositories;
using Fsel.Training.Domain.Models.EntityModels;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Training.Application.Queues.Consumers
{
    public class UpdateClassLiveAssignmentConsumer : IConsumer<BaseQueueModel>
    {
        private readonly IClassRepository _classRepository;
        private readonly IClassLiveWorkFlowRepository _classLiveWorkFlowRepository;
        private readonly IClassLiveCalendarRepository _classLiveCalendarRepository;

        public UpdateClassLiveAssignmentConsumer(
            IClassRepository classRepository,
            IClassLiveWorkFlowRepository classLiveWorkFlowRepository,
            IClassLiveCalendarRepository classLiveCalendarRepository)
        {
            _classRepository = classRepository;
            _classLiveWorkFlowRepository = classLiveWorkFlowRepository;
            _classLiveCalendarRepository = classLiveCalendarRepository;
        }

        public async Task Consume(ConsumeContext<BaseQueueModel> context)
        {
            var classPendingTeachers = await _classRepository.Queryable.Where(x => x.TeacherApprovalStatus == EnumTeacherApprovalStatus.Pending && x.TeacherId != null).ToListAsync();
            List<ClassLiveCalendar> classLiveCalendars = new List<ClassLiveCalendar>();
            List<Class> classPendings = new List<Class>();
            foreach (var classPending in classPendingTeachers)
            {
                var classLiveCalendar = await _classLiveCalendarRepository.Queryable.Where(x => x.ClassId == classPending.Id && x.Status == EnumClassLiveCalendarStatus.NotStudied)
                    .OrderByDescending(x => x.LiveDate).FirstOrDefaultAsync();
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
                }).ToListAsync();
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
                    if (item.StartDate.Value.Date == DateTime.Now.Date.AddDays(1))
                    {
                        ids.Add(item.Id);
                    }
                }
            }
            if (ids.Count > 0)
            {
                var classLiveWordFlows = await _classLiveWorkFlowRepository.Queryable.Include(x => x.ClassLiveCalendar).Where(x => ids.Contains(x.Id)).ToListAsync();
                var classes = await _classRepository.Queryable.Include(x => x.ClassLiveCalendars).Where(x => ids.Contains(x.Id)).ToListAsync();
                if (classes.Count > 0)
                {
                    classes.ForEach(x =>
                    {
                        if (x.Status == EnumStatusClass.Active)
                        {
                            var classLiveCalendars = x.ClassLiveCalendars.Where(classLive => classLive.LiveDate.Date > DateTime.Now)
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
                    await _classRepository.UnitOfWork.SaveEntitiesAsync().ConfigureAwait(false);
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
                    await _classLiveWorkFlowRepository.UnitOfWork.SaveEntitiesAsync().ConfigureAwait(false);
                }
            }
        }
    }
}
