// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Master.Application.Queries.StudentDashboard
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Extensions;
    using Fsel.Master.Domain.Entities;
    using Fsel.Master.Domain.IRepositories;
    using Fsel.Master.Domain.Models.EntityModels.StudentDashboard;
    using Fsel.Master.Domain.Models.Enums;
    using Fsel.Master.Domain.Models.QueryModels.StudentDashboard;
    using Fsel.Master.Infrastructure;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetCourseCompletionQueryHandler : IRequestHandler<GetCourseCompletionQuery, MethodResult<CourseCompletionResponseModel>>
    {
        private readonly MasterDBContext _dbContext;
        private readonly IMasterBaseRepository<Course> _repository;

        public GetCourseCompletionQueryHandler(MasterDBContext dbContext, IMasterBaseRepository<Course> repository)
        {
            _dbContext = dbContext;
            _repository = repository;
        }

        public async Task<MethodResult<CourseCompletionResponseModel>> Handle(GetCourseCompletionQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<CourseCompletionResponseModel>();

            // === Base course result query - Status = Done, WorkingStatus = Active ===
            IQueryable<CourseResult> courseQuery = _dbContext.CourseResults
                .Where(cr => cr.Status == EnumResultStatus.Done && cr.WorkingStatus == EnumWorkingStatus.Active);

            if (request.CourseIds?.Any() == true)
            {
                courseQuery = courseQuery.Where(cr => request.CourseIds.Contains(cr.CourseId));
            }
            if (request.FromDate.HasValue)
            {
                courseQuery = courseQuery.Where(cr => cr.CreatedDate >= request.FromDate.Value);
            }
            if (request.ToDate.HasValue)
            {
                courseQuery = courseQuery.Where(cr => cr.CreatedDate <= request.ToDate.Value);
            }

            // Join with StudentProfile for location filter
            var studentBaseQuery = from cr in courseQuery
                                   join s in _dbContext.StudentProfileReports
                                       on cr.StudentId equals s.StudentId
                                   join course in _dbContext.Courses
                                       on cr.CourseId equals course.CourseId into courseJoin
                                   from course in courseJoin.DefaultIfEmpty()
                                   join level in _dbContext.Levels on course.LevelId equals level.LevelId into levelJoin
                                   from level in levelJoin.DefaultIfEmpty()
                                   where s.ProvinceId != null && s.ProvinceId != Guid.Empty
                                       && s.DistrictId != null && s.DistrictId != Guid.Empty
                                       && s.SchoolId != null && s.SchoolId != Guid.Empty
                                   select new { cr, s, course, level };

            if (request.ProvinceIds?.Any() == true)
            {
                studentBaseQuery = studentBaseQuery.Where(x => x.s.ProvinceId != null && request.ProvinceIds.Contains(x.s.ProvinceId.Value));
            }

            if (request.DistrictIds?.Any() == true)
            {
                studentBaseQuery = studentBaseQuery.Where(x => x.s.DistrictId != null && request.DistrictIds.Contains(x.s.DistrictId.Value));
            }
            if (request.SchoolIds?.Any() == true)
            {
                studentBaseQuery = studentBaseQuery.Where(x => x.s.SchoolId != null && request.SchoolIds.Contains(x.s.SchoolId.Value));
            }

            // === KPI: Total entered students ===
            var totalEnteredStudents = await studentBaseQuery
                .Select(x => x.cr.StudentId)
                .Distinct()
                .CountAsync(cancellationToken);

            // === KPI: Total completed students (CompletedLessons >= TotalLessons) ===
            var totalCompletedStudents = await studentBaseQuery
                .Where(x => x.cr.CompletedLessons >= x.cr.TotalLessons)
                .Select(x => x.cr.StudentId)
                .Distinct()
                .CountAsync(cancellationToken);

            // === KPI: Completion rate ===
            var completionRate = totalEnteredStudents == 0 ? 0 : Math.Round((double)totalCompletedStudents / totalEnteredStudents * 100, 2);

            // === Top 10 students completed - database-level ===
            var topStudents = await (from x in studentBaseQuery
                                     where x.cr.CompletedLessons >= x.cr.TotalLessons
                                     let totalAccessTime = _dbContext.LearningActivities
                                         .Where(la => la.StudentId == x.cr.StudentId
                                                      && la.CourseId == x.cr.CourseId
                                                      && la.Feature != EnumFeature.Other
                                                      && la.LastVisited.HasValue
                                                      && (la.LastVisited < x.cr.CompletionDate || la.LastVisited < x.cr.UpdatedDate))
                                         .Sum(la => la.AccessTime)
                                     select new CourseCompletionTopStudentModel
                                     {
                                         StudentId = x.cr.StudentId,
                                         FullName = x.s.FullName,
                                         SchoolName = x.s.SchoolName,
                                         ProvinceName = x.s.ProvinceName,
                                         CourseName = x.course != null ? x.course.CourseName : null,
                                         LevelName = x.level != null ? x.level.LevelName : null,
                                         CompletedLessons = x.cr.CompletedLessons,
                                         TargetLessons = x.cr.TotalLessons,
                                         Percent = x.cr.Percent,
                                         CompletedDate = x.cr.CompletionDate,
                                         TotalAccessTime = totalAccessTime
                                     })
                                     .OrderBy(x => x.TotalAccessTime)
                                     .ApplyPaging(request)
                                     .ToListAsync(cancellationToken);

            methodResult.Result = new CourseCompletionResponseModel
            {
                TotalCompletedStudents = totalCompletedStudents,
                TotalEnteredStudents = totalEnteredStudents,
                CompletionRate = completionRate,
                TopStudents = topStudents
            };

            return methodResult;
        }
    }
}
