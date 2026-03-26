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

    public class GetEntranceExamOverviewQueryHandler : IRequestHandler<GetEntranceExamOverviewQuery, MethodResult<EntranceExamOverviewModel>>
    {
        private readonly MasterDBContext _dbContext;

        public GetEntranceExamOverviewQueryHandler(MasterDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<MethodResult<EntranceExamOverviewModel>> Handle(GetEntranceExamOverviewQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<EntranceExamOverviewModel>();

            // === Base student query - IQueryable, KHONG ToList() ===
            IQueryable<Guid> studentIdQuery = _dbContext.StudentProfileReports
                .Join(_dbContext.StudentCompetitionEvents,
                    s => s.StudentId, sce => sce.StudentId, (s, sce) => s.StudentId)
                .Where(s => _dbContext.StudentProfileReports.Any(sp => sp.StudentId == s
                    && sp.ProvinceId != null && sp.DistrictId != null && sp.SchoolId != null
                    && sp.ProvinceId != Guid.Empty && sp.DistrictId != Guid.Empty && sp.SchoolId != Guid.Empty));

            if (request.ProvinceIds?.Any() == true)
            {
                studentIdQuery = studentIdQuery.Where(sid => _dbContext.StudentProfileReports
                  .Any(sp => sp.StudentId == sid && sp.ProvinceId != null && request.ProvinceIds.Contains(sp.ProvinceId.Value)));
            }

            if (request.DistrictIds?.Any() == true)
            {
                studentIdQuery = studentIdQuery.Where(sid => _dbContext.StudentProfileReports
                    .Any(sp => sp.StudentId == sid && sp.DistrictId != null && request.DistrictIds.Contains(sp.DistrictId.Value)));
            }

            if (request.SchoolIds?.Any() == true)
            {
                studentIdQuery = studentIdQuery.Where(sid => _dbContext.StudentProfileReports
                    .Any(sp => sp.StudentId == sid && sp.SchoolId != null && request.SchoolIds.Contains(sp.SchoolId.Value)));
            }

            if (request.SubjectIds?.Any() == true)
            {
                var subjectStudentIds = from sp in _dbContext.StudentProfileReports
                                        join sce in _dbContext.StudentCompetitionEvents on sp.StudentId equals sce.StudentId
                                        join prog in _dbContext.Programs on sce.CompetitionEventId equals prog.ProgramId
                                        where request.SubjectIds.Contains(prog.SubjectId)
                                        select sp.StudentId;
                studentIdQuery = studentIdQuery.Where(sid => subjectStudentIds.Contains(sid));
            }

            if (request.FromDate.HasValue)
            {
                var fromDateIds = _dbContext.StudentCompetitionEvents
                    .Where(x => x.CreatedDate >= request.FromDate.Value)
                    .Select(x => x.StudentId);
                studentIdQuery = studentIdQuery.Where(sid => fromDateIds.Contains(sid));
            }

            if (request.ToDate.HasValue)
            {
                var toDateIds = _dbContext.StudentCompetitionEvents
                    .Where(x => x.CreatedDate <= request.ToDate.Value)
                    .Select(x => x.StudentId);
                studentIdQuery = studentIdQuery.Where(sid => toDateIds.Contains(sid));
            }

            // Chua ToList() - van la IQueryable

            // === TotalStudents - database COUNT ===
            var totalStudents = await studentIdQuery.Distinct().CountAsync(cancellationToken);

            // === Placement exam counts - database GROUP BY ===
            var placementCounts = await (from p in _dbContext.PlacementTestReports
                                         where studentIdQuery.Any(sid => sid == p.StudentId)
                                         group p by p.Status into g
                                         select new
                                         {
                                             Status = g.Key,
                                             Count = g.Select(x => x.StudentId).Distinct().Count()
                                         })
                                        .ToListAsync(cancellationToken);

            var completedExam = placementCounts.FirstOrDefault(x => x.Status == EnumResultStatus.Done)?.Count ?? 0;
            var inProgressExam = placementCounts.FirstOrDefault(x => x.Status == EnumResultStatus.Process)?.Count ?? 0;
            var studentsWithPlacementRecord = placementCounts.Sum(x => x.Count);
            var notRegisteredExam = totalStudents - studentsWithPlacementRecord;
            // NotStarted = hoc vien co placement record nhung chua bat dau lam (Tong co placement - Da hoan thanh - Dang lam)
            var notStartedExam = studentsWithPlacementRecord - completedExam - inProgressExam;

            methodResult.Result = new EntranceExamOverviewModel
            {
                TotalStudents = totalStudents,
                Labels = new List<string>
                {
                    "Hoàn thành bài kiểm tra",
                    "Đang làm bài kiểm tra",
                    "Chưa làm bài kiểm tra",
                    "Chưa đăng ký"
                },
                Data = new List<int>
                {
                    completedExam,
                    inProgressExam,
                    notStartedExam,
                    notRegisteredExam
                }
            };

            return methodResult;
        }
    }
}
