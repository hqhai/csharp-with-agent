// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Master.Application.Queries.StudentDashboard
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Master.Domain.Entities;
    using Fsel.Master.Domain.Models.EntityModels.StudentDashboard;
    using Fsel.Master.Domain.Models.Enums;
    using Fsel.Master.Domain.Models.QueryModels.StudentDashboard;
    using Fsel.Master.Infrastructure;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentDashboardSummaryQueryHandler : IRequestHandler<GetStudentDashboardSummaryQuery, MethodResult<StudentDashboardSummaryModel>>
    {
        private readonly MasterDBContext _dbContext;

        public GetStudentDashboardSummaryQueryHandler(MasterDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<MethodResult<StudentDashboardSummaryModel>> Handle(GetStudentDashboardSummaryQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<StudentDashboardSummaryModel>();

            // === Base student query - IQueryable, KHONG ToList() ===
            IQueryable<StudentProfileReport> studentQuery = _dbContext.StudentProfileReports
                .Join(_dbContext.StudentCompetitionEvents, s => s.StudentId, sce => sce.StudentId, (s, sce) => s)
                .Where(s => s.ProvinceId != null && s.DistrictId != null && s.SchoolId != null)
                .Where(s => s.ProvinceId != Guid.Empty && s.DistrictId != Guid.Empty && s.SchoolId != Guid.Empty);

            if (request.ProvinceIds?.Any() == true)
            {
                studentQuery = studentQuery.Where(s => s.ProvinceId != null && request.ProvinceIds.Contains(s.ProvinceId.Value));
            }
            if (request.DistrictIds?.Any() == true)
            {
                studentQuery = studentQuery.Where(s => s.DistrictId != null && request.DistrictIds.Contains(s.DistrictId.Value));
            }
            if (request.SchoolIds?.Any() == true)
            {
                studentQuery = studentQuery.Where(s => s.SchoolId != null && request.SchoolIds.Contains(s.SchoolId.Value));
            }
            if (request.SubjectIds?.Any() == true)
            {
                studentQuery = from s in studentQuery
                               join sce in _dbContext.StudentCompetitionEvents on s.StudentId equals sce.StudentId
                               join prog in _dbContext.Programs on sce.CompetitionEventId equals prog.ProgramId
                               where request.SubjectIds.Contains(prog.SubjectId)
                               select s;
            }

            if (request.FromDate.HasValue)
            {
                var fromDateIds = _dbContext.StudentCompetitionEvents
                    .Where(x => x.CreatedDate >= request.FromDate.Value)
                    .Select(x => x.StudentId);
                studentQuery = studentQuery.Where(s => fromDateIds.Contains(s.StudentId));
            }

            if (request.ToDate.HasValue)
            {
                var toDateIds = _dbContext.StudentCompetitionEvents
                    .Where(x => x.CreatedDate <= request.ToDate.Value)
                    .Select(x => x.StudentId);
                studentQuery = studentQuery.Where(s => toDateIds.Contains(s.StudentId));
            }

            // studentQuery la IQueryable - chua chay xuong DB

            // === 1. TotalStudents: database COUNT ===
            var totalStudents = await studentQuery.Select(s => s.StudentId).Distinct().CountAsync(cancellationToken);

            // === 2. TotalAccounts: database COUNT DISTINCT ===
            var totalAccounts = await studentQuery.Select(s => s.UserId).Distinct().CountAsync(cancellationToken);

            // === 3. Placement exam stats - TAT CA database aggregation ===
            // 3a. Completed
            var completedExam = await studentQuery
                .Join(_dbContext.PlacementTestReports,
                    s => s.StudentId, p => p.StudentId, (s, p) => p)
                .Where(p => p.Status == EnumResultStatus.Done)
                .Select(p => p.StudentId)
                .Distinct()
                .CountAsync(cancellationToken);

            // 3b. InProgress
            var inProgressExam = await studentQuery
                .Join(_dbContext.PlacementTestReports,
                    s => s.StudentId, p => p.StudentId, (s, p) => p)
                .Where(p => p.Status == EnumResultStatus.Process)
                .Select(p => p.StudentId)
                .Distinct()
                .CountAsync(cancellationToken);

            // 3c. NotRegistered = hoc vien chua co placement record
            var studentsWithPlacementRecord = await studentQuery
                .Join(_dbContext.PlacementTestReports,
                    s => s.StudentId, p => p.StudentId, (s, p) => p)
                .Select(p => p.StudentId)
                .Distinct()
                .CountAsync(cancellationToken);

            var notRegisteredExam = totalStudents - studentsWithPlacementRecord;

            // 3d. NotStarted = hoc vien co placement record nhung chua lam (tru di da hoan thanh va dang lam)
            var notStartedExam = studentsWithPlacementRecord - completedExam - inProgressExam;

            // === 4. Learning progress - database aggregation ===
            // Lay snapshot cuoi cung (nhieu record moi hoc vien) - database-level
            var latestProgressStats = await _dbContext.StudentLearningProgresses
                .Where(lp => studentQuery.Select(s => s.StudentId).Contains(lp.StudentId))
                .GroupBy(lp => lp.StudentId)
                .Select(g => g
                    .OrderByDescending(x => x.TotalCompletedLessons)
                    .Select(x => new
                    {
                        StudentId = x.StudentId,
                        x.CurrentProgressStatus,
                        x.TotalCompletedLessons,
                        x.TotalTargetLessons
                    })
                    .First())
                .ToListAsync(cancellationToken);

            var studentsEnteredLearning = latestProgressStats.Count(x => x.TotalCompletedLessons > 0);
            var studentsOnTrack = latestProgressStats.Count(x => x.CurrentProgressStatus == EnumCurrentProgressStatus.OnTarget);
            var studentsBehindTrack = latestProgressStats.Count(x => x.CurrentProgressStatus == EnumCurrentProgressStatus.BelowTarget);
            var studentsCompletedLearning = latestProgressStats.Count(x => x.TotalTargetLessons > 0 && x.TotalCompletedLessons >= x.TotalTargetLessons);
            var studentsNotCompletedLearning = studentsEnteredLearning - studentsCompletedLearning;

            // === Rate calculations ===
            var total = totalStudents > 0 ? totalStudents : 1;
            var enteredLearningTotal = studentsEnteredLearning > 0 ? studentsEnteredLearning : 1;

            // Entrance exam count: lấy phần còn lại để tránh lệch
            notRegisteredExam = Math.Max(0, totalStudents - completedExam - inProgressExam - notStartedExam);

            // Entrance exam rate: tính 3 cái đầu trước, cái cuối lấy phần còn lại để đủ 100%
            var completedEntranceExamRate = NumberHelper.GetPercent(completedExam, total, 2);
            var inProgressEntranceExamRate = NumberHelper.GetPercent(inProgressExam, total, 2);
            var notStartedEntranceExamRate = NumberHelper.GetPercent(notStartedExam, total, 2);
            var notRegisteredEntranceExamRate = Math.Max(
                0,
                Math.Round(100 - completedEntranceExamRate - inProgressEntranceExamRate - notStartedEntranceExamRate, 2));

            methodResult.Result = new StudentDashboardSummaryModel
            {
                // Count fields
                TotalStudents = totalStudents,
                TotalAccounts = totalAccounts,
                CompletedEntranceExam = completedExam,
                InProgressEntranceExam = inProgressExam,
                NotStartedEntranceExam = notStartedExam,
                NotRegisteredEntranceExam = notRegisteredExam,
                StudentsEnteredLearning = studentsEnteredLearning,
                StudentsOnTrack = studentsOnTrack,
                StudentsBehindTrack = studentsBehindTrack,
                StudentsCompletedLearning = studentsCompletedLearning,
                StudentsNotCompletedLearning = studentsNotCompletedLearning,

                // Rate: base metrics (% trên totalStudents)
                TotalStudentsRate = NumberHelper.GetPercent(totalStudents, total, 2),
                TotalAccountsRate = NumberHelper.GetPercent(totalAccounts, total, 2),

                // Rate: entrance exam (% trên totalStudents)
                CompletedEntranceExamRate = completedEntranceExamRate,
                InProgressEntranceExamRate = inProgressEntranceExamRate,
                NotStartedEntranceExamRate = notStartedEntranceExamRate,
                NotRegisteredEntranceExamRate = notRegisteredEntranceExamRate,

                // Rate: learning progress
                StudentsEnteredLearningRate = NumberHelper.GetPercent(studentsEnteredLearning, total, 2),
                StudentsOnTrackRate = NumberHelper.GetPercent(studentsOnTrack, enteredLearningTotal, 2),
                StudentsBehindTrackRate = NumberHelper.GetPercent(studentsBehindTrack, enteredLearningTotal, 2),
                StudentsCompletedLearningRate = NumberHelper.GetPercent(studentsCompletedLearning, enteredLearningTotal, 2),
                StudentsNotCompletedLearningRate = NumberHelper.GetPercent(studentsNotCompletedLearning, enteredLearningTotal, 2)
            };

            return methodResult;
        }
    }
}
