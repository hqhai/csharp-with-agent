// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Master.Application.Queries.StudentDashboard
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Master.Domain.Entities;
    using Fsel.Master.Domain.Models.EntityModels.StudentDashboard;
    using Fsel.Master.Domain.Models.QueryModels.StudentDashboard;
    using Fsel.Master.Infrastructure;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetWeeklyProgressQueryHandler : IRequestHandler<GetWeeklyProgressQuery, MethodResult<IList<WeeklyProgressModel>>>
    {
        private readonly MasterDBContext _dbContext;

        public GetWeeklyProgressQueryHandler(MasterDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<MethodResult<IList<WeeklyProgressModel>>> Handle(
            GetWeeklyProgressQuery request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var methodResult = new MethodResult<IList<WeeklyProgressModel>>();

            // Base student profile query
            var studentProfiles = _dbContext.StudentProfileReports
                .AsNoTracking()
                .Where(x =>
                    x.StudentId != Guid.Empty &&
                    x.ProvinceId.HasValue &&
                    x.DistrictId.HasValue &&
                    x.SchoolId.HasValue &&
                    x.ProvinceId.Value != Guid.Empty &&
                    x.DistrictId.Value != Guid.Empty &&
                    x.SchoolId.Value != Guid.Empty)
                .AsQueryable();

            if (request.ProvinceIds?.Any() == true)
            {
                studentProfiles = studentProfiles
                    .Where(x => x.ProvinceId.HasValue && request.ProvinceIds.Contains(x.ProvinceId.Value));
            }

            if (request.DistrictIds?.Any() == true)
            {
                studentProfiles = studentProfiles
                    .Where(x => x.DistrictId.HasValue && request.DistrictIds.Contains(x.DistrictId.Value));
            }

            if (request.SchoolIds?.Any() == true)
            {
                studentProfiles = studentProfiles
                    .Where(x => x.SchoolId.HasValue && request.SchoolIds.Contains(x.SchoolId.Value));
            }

            // Students must have joined/entered learning event
            var studentCompetitionEvents = _dbContext.StudentCompetitionEvents
                .AsNoTracking()
                .AsQueryable();

            if (request.FromDate.HasValue)
            {
                var fromDate = request.FromDate.Value;
                studentCompetitionEvents = studentCompetitionEvents
                    .Where(x => x.CreatedDate >= fromDate);
            }

            if (request.ToDate.HasValue)
            {
                var toDate = request.ToDate.Value;
                studentCompetitionEvents = studentCompetitionEvents
                    .Where(x => x.CreatedDate <= toDate);
            }

            IQueryable<Guid> studentIdQuery =
                from sp in studentProfiles
                join sce in studentCompetitionEvents on sp.StudentId equals sce.StudentId
                select sp.StudentId;

            if (request.SubjectIds?.Any() == true)
            {
                var subjectStudentIds =
                    from sce in _dbContext.StudentCompetitionEvents.AsNoTracking()
                    join prog in _dbContext.Programs.AsNoTracking()
                        on sce.CompetitionEventId equals prog.ProgramId
                    where request.SubjectIds.Contains(prog.SubjectId)
                    select sce.StudentId;

                studentIdQuery = studentIdQuery.Where(x => subjectStudentIds.Contains(x));
            }

            studentIdQuery = studentIdQuery.Distinct();

            var weeklyStats = await _dbContext.StudentWeeklyLearningProgresses
                .AsNoTracking()
                .Where(wp => studentIdQuery.Contains(wp.StudentId))
                .GroupBy(wp => new
                {
                    wp.WeekStartDate,
                    wp.WeekEndDate,
                    wp.WeekIndex,
                })
                .Select(g => new
                {
                    g.Key.WeekStartDate,
                    g.Key.WeekEndDate,
                    g.Key.WeekIndex,
                    TotalStudents = g.Select(x => x.StudentId).Distinct().Count(),
                    OnTrack = g.Count(x => x.ProgressStatus == EnumCurrentProgressStatus.OnTarget),
                    Behind = g.Count(x => x.ProgressStatus == EnumCurrentProgressStatus.BelowTarget),
                    Completed = g.Count(x => x.WeeklyTargetLessons > 0 && x.CompletedLessons >= x.WeeklyTargetLessons),
                })
                .OrderBy(x => x.WeekStartDate)
                .ToListAsync(cancellationToken);

            methodResult.Result = weeklyStats
                .Select(x => new WeeklyProgressModel
                {
                    WeekStartDate = x.WeekStartDate,
                    WeekEndDate = x.WeekEndDate,
                    WeekIndex = x.WeekIndex,
                    TotalStudents = x.TotalStudents,
                    OnTrackPercentage = NumberHelper.GetPercent(x.OnTrack, x.TotalStudents, 2),
                    BehindPercentage = NumberHelper.GetPercent(x.Behind, x.TotalStudents, 2),
                    CompletedPercentage = NumberHelper.GetPercent(x.Completed, x.TotalStudents, 2),
                })
                .ToList();

            return methodResult;
        }
    }
}
