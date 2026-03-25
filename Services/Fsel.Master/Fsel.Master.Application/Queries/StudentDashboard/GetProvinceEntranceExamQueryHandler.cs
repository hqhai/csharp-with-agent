// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Master.Application.Queries.StudentDashboard
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Master.Domain.Models.EntityModels.StudentDashboard;
    using Fsel.Master.Domain.Models.Enums;
    using Fsel.Master.Domain.Models.QueryModels.StudentDashboard;
    using Fsel.Master.Infrastructure;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetProvinceEntranceExamQueryHandler : IRequestHandler<GetProvinceEntranceExamQuery, MethodResult<IList<ProvinceEntranceExamModel>>>
    {
        private readonly MasterDBContext _dbContext;

        public GetProvinceEntranceExamQueryHandler(MasterDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<MethodResult<IList<ProvinceEntranceExamModel>>> Handle(GetProvinceEntranceExamQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<ProvinceEntranceExamModel>>();

            // === Base student query - IQueryable, KHONG ToList() ===
            var provinceStudentQuery =
                from s in _dbContext.StudentProfileReports
                join sce in _dbContext.StudentCompetitionEvents on s.StudentId equals sce.StudentId
                where s.ProvinceId != null
                && s.DistrictId != null
                && s.SchoolId != null
                && s.ProvinceId != Guid.Empty
                && s.DistrictId != Guid.Empty
                && s.SchoolId != Guid.Empty
                select new { s.StudentId, s.ProvinceId, s.ProvinceName };

            if (request.ProvinceIds?.Any() == true)
            {
                provinceStudentQuery = provinceStudentQuery.Where(x => x.ProvinceId != null && request.ProvinceIds.Contains(x.ProvinceId.Value));
            }

            if (request.DistrictIds?.Any() == true)
            {
                provinceStudentQuery = provinceStudentQuery.Where(x =>
                _dbContext.StudentProfileReports.Any(s => s.StudentId == x.StudentId && s.DistrictId != null && request.DistrictIds.Contains(s.DistrictId.Value)));
            }

            if (request.SchoolIds?.Any() == true)
            {
                provinceStudentQuery = provinceStudentQuery.Where(x =>
                   _dbContext.StudentProfileReports.Any(s => s.StudentId == x.StudentId && s.SchoolId != null && request.SchoolIds.Contains(s.SchoolId.Value)));
            }

            if (request.SubjectIds?.Any() == true)
            {
                var subjectStudentIds = from sp in _dbContext.StudentProfileReports
                                        join sce in _dbContext.StudentCompetitionEvents on sp.StudentId equals sce.StudentId
                                        join prog in _dbContext.Programs on sce.CompetitionEventId equals prog.ProgramId
                                        where request.SubjectIds.Contains(prog.SubjectId)
                                        select sp.StudentId;
                provinceStudentQuery = provinceStudentQuery.Where(x => subjectStudentIds.Contains(x.StudentId));
            }

            if (request.FromDate.HasValue)
            {
                var fromDateIds = _dbContext.StudentCompetitionEvents
                    .Where(x => x.CreatedDate >= request.FromDate.Value)
                    .Select(x => x.StudentId);
                provinceStudentQuery = provinceStudentQuery.Where(x => fromDateIds.Contains(x.StudentId));
            }

            if (request.ToDate.HasValue)
            {
                var toDateIds = _dbContext.StudentCompetitionEvents
                    .Where(x => x.CreatedDate <= request.ToDate.Value)
                    .Select(x => x.StudentId);
                provinceStudentQuery = provinceStudentQuery.Where(x => toDateIds.Contains(x.StudentId));
            }

            // === Database-level GROUP BY theo Province + Placement Status ===
            // Lay TotalStudents + Completed/InProgress/NotStarted count theo province trong 1 query
            var provinceData = await (from ps in provinceStudentQuery
                                      join p in _dbContext.PlacementTestReports on ps.StudentId equals p.StudentId into gj
                                      from p in gj.DefaultIfEmpty()
                                      select new
                                      {
                                          ps.ProvinceId,
                                          ps.ProvinceName,
                                          ps.StudentId,
                                          p.Status
                                      })
                                      .ToListAsync(cancellationToken);

            // Group by province trong memory - chi lay aggregated counts
            var result = provinceData
                .GroupBy(x => new { x.ProvinceId, x.ProvinceName })
                .Select(g =>
                {
                    var totalStudents = g.Count();
                    var completed = g.Count(x => x.Status == EnumResultStatus.Done);
                    var inProgress = g.Count(x => x.Status == EnumResultStatus.Process);
                    var notStarted = g.Count(x => x.Status != null && x.Status != EnumResultStatus.Done && x.Status != EnumResultStatus.Process);
                    var notRegistered = g.Count(x => x.Status == null);

                    return new
                    {
                        g.Key.ProvinceId,
                        g.Key.ProvinceName,
                        TotalStudents = totalStudents,
                        Completed = completed,
                        InProgress = inProgress,
                        NotStarted = notStarted,
                        NotRegistered = notRegistered
                    };
                })
                .Where(x => x.ProvinceId != null && x.ProvinceId != Guid.Empty)
                .OrderByDescending(x => x.TotalStudents)
                .Select(x => new ProvinceEntranceExamModel
                {
                    ProvinceId = x.ProvinceId ?? Guid.Empty,
                    ProvinceName = x.ProvinceName,
                    TotalStudents = x.TotalStudents,
                    CompletedPercentage = x.TotalStudents == 0 ? 0 : Math.Round((double)x.Completed / x.TotalStudents * 100, 2),
                    InProgressPercentage = x.TotalStudents == 0 ? 0 : Math.Round((double)x.InProgress / x.TotalStudents * 100, 2),
                    NotStartedPercentage = x.TotalStudents == 0 ? 0 : Math.Round((double)x.NotStarted / x.TotalStudents * 100, 2),
                    NotRegisteredPercentage = x.TotalStudents == 0 ? 0 : Math.Round((double)x.NotRegistered / x.TotalStudents * 100, 2)
                })
                .ToList();

            methodResult.Result = result;
            return methodResult;
        }
    }
}
