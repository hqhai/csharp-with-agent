// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Master.Application.Queries.ProgressMetrics
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Master.Domain.Entities;
    using Fsel.Master.Domain.IRepositories;
    using Fsel.Master.Domain.Models.Enums;
    using Fsel.Master.Domain.Models.QueryModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GoalOverviewModel
    {
        public int TotalLearn { get; set; }
        public int TotalDoneUnit { get; set; }
        public int TotalDoneLesson { get; set; }
        public int TotalDoneCourse { get; set; }
    }

    public class GetGoalOverviewQuery : BaseProgressMetricsQueryModel, IRequest<MethodResult<GoalOverviewModel>>
    {
    }

    public class GetGoalOverviewQueryHandler : IRequestHandler<GetGoalOverviewQuery, MethodResult<GoalOverviewModel>>
    {
        private readonly IMasterBaseRepository<StudentProfileReport> _studentRepository;
        private readonly IMasterBaseRepository<StudentCompetitionEvent> _studentCompetitionEventRepository;
        private readonly IMasterBaseRepository<CourseResult> _courseResultRepository;
        private readonly IMasterBaseRepository<LessonResult> _lessonResultRepository;
        private readonly IMasterBaseRepository<Course> _courseRepository;
        private readonly IMasterBaseRepository<Level> _levelRepository;
        private readonly IMasterBaseRepository<Program> _programRepository;
        private readonly IMasterBaseRepository<UnitResult> _unitResultRepository;

        public GetGoalOverviewQueryHandler(IMasterBaseRepository<StudentProfileReport> studentRepository, IMasterBaseRepository<StudentCompetitionEvent> studentCompetitionEventRepository, IMasterBaseRepository<CourseResult> courseResultRepository, IMasterBaseRepository<LessonResult> lessonResultRepository, IMasterBaseRepository<Course> courseRepository, IMasterBaseRepository<Level> levelRepository, IMasterBaseRepository<Program> programRepository, IMasterBaseRepository<UnitResult> unitResultRepository)
        {
            _studentRepository = studentRepository;
            _studentCompetitionEventRepository = studentCompetitionEventRepository;
            _courseResultRepository = courseResultRepository;
            _lessonResultRepository = lessonResultRepository;
            _courseRepository = courseRepository;
            _levelRepository = levelRepository;
            _programRepository = programRepository;
            _unitResultRepository = unitResultRepository;
        }

        public async Task<MethodResult<GoalOverviewModel>> Handle(GetGoalOverviewQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<GoalOverviewModel>();

            var studentsQuery = from s in _studentRepository.Queryable
                                where s.ProvinceId.HasValue && s.ProvinceId != default
                                   && s.DistrictId.HasValue && s.DistrictId != default
                                   && s.SchoolId.HasValue && s.SchoolId != default
                                select s;

            if (request.ProvinceIds?.Any() == true)
            {
                studentsQuery = studentsQuery.Where(s => s.ProvinceId.HasValue && request.ProvinceIds.Contains(s.ProvinceId.Value));
            }

            if (request.DistrictIds?.Any() == true)
            {
                studentsQuery = studentsQuery.Where(s => s.DistrictId.HasValue && request.DistrictIds.Contains(s.DistrictId.Value));
            }

            if (request.SchoolIds?.Any() == true)
            {
                studentsQuery = studentsQuery.Where(s => s.SchoolId.HasValue && request.SchoolIds.Contains(s.SchoolId.Value));
            }

            var studentCompetitionQuery = from s in studentsQuery
                                          join sce in _studentCompetitionEventRepository.Queryable
                                              on s.StudentId equals sce.StudentId
                                          select new
                                          {
                                              s.StudentId,
                                              s.ProvinceId,
                                              s.DistrictId,
                                              s.SchoolId,
                                              sce.CompetitionEventId
                                          };

            var courseQuery = from sc in studentCompetitionQuery
                              join cr in _courseResultRepository.Queryable on sc.StudentId equals cr.StudentId
                              join c in _courseRepository.Queryable on cr.CourseId equals c.CourseId
                              where cr.WorkingStatus == EnumWorkingStatus.Active && c.SubjectId == request.SubjectId
                              select new { sc.StudentId, cr.CourseResultId, c.LevelId, cr.Percent, c.ProgramId, cr.Status };

            if (request.ProgramIds != null && request.ProgramIds.Count > 0)
            {
                courseQuery = courseQuery.Where(p => request.ProgramIds.Contains(p.ProgramId));
            }

            if (request.LevelIds != null && request.LevelIds.Count > 0)
            {
                courseQuery = courseQuery.Where(p => request.LevelIds.Contains(p.LevelId));
            }

            var validCourseIds = courseQuery.Select(x => x.CourseResultId).Distinct();

            var totalStats = await courseQuery
                .GroupBy(x => 1)
                .Select(g => new
                {
                    TotalLearn = g.Select(x => x.CourseResultId).Distinct().Count(),
                    TotalDoneCourse = g.Where(x => x.Status == EnumResultStatus.Done).Select(x => x.CourseResultId).Distinct().Count()
                })
                .FirstOrDefaultAsync(cancellationToken);

            var doneLessonCount = await _lessonResultRepository.Queryable
                .Where(l => l.Status == EnumResultStatus.Done && validCourseIds.Contains(l.CourseResultId))
                .Select(l => l.CourseResultId)
                .Distinct()
                .CountAsync(cancellationToken);

            var doneUnitCount = await _unitResultRepository.Queryable
                .Where(u => u.Status == EnumResultStatus.Done && u.CourseResultId.HasValue && validCourseIds.Contains(u.CourseResultId.Value))
                .Select(u => u.CourseResultId)
                .Distinct()
                .CountAsync(cancellationToken);

            // 4. Tổng hợp kết quả
            methodResult.Result = new GoalOverviewModel
            {
                TotalLearn = totalStats?.TotalLearn ?? 0,
                TotalDoneCourse = totalStats?.TotalDoneCourse ?? 0,
                TotalDoneLesson = doneLessonCount,
                TotalDoneUnit = doneUnitCount
            };

            return methodResult;
        }
    }
}
