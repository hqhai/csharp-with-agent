// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Master.Application.Queries.ProgressMetrics
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Master.Domain.Entities;
    using Fsel.Master.Domain.IRepositories;
    using Fsel.Master.Domain.Models.QueryModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetLearningGoalResultQuery : BaseProgressMetricsQueryModel, IRequest<MethodResult<IList<GoalProgressModel>>>
    {
    }

    public class GetLearningGoalResultQueryHandler : IRequestHandler<GetLearningGoalResultQuery, MethodResult<IList<GoalProgressModel>>>
    {
        private readonly IMasterBaseRepository<StudentProfileReport> _studentRepository;
        private readonly IMasterBaseRepository<StudentCompetitionEvent> _studentCompetitionEventRepository;
        private readonly IMasterBaseRepository<CourseResult> _courseResultRepository;
        private readonly IMasterBaseRepository<LessonResult> _lessonResultRepository;
        private readonly IMasterBaseRepository<Course> _courseRepository;
        private readonly IMasterBaseRepository<Level> _levelRepository;
        private readonly IMasterBaseRepository<Program> _programRepository;

        public GetLearningGoalResultQueryHandler(IMasterBaseRepository<StudentProfileReport> studentRepository, IMasterBaseRepository<StudentCompetitionEvent> studentCompetitionEventRepository, IMasterBaseRepository<CourseResult> courseResultRepository, IMasterBaseRepository<LessonResult> lessonResultRepository, IMasterBaseRepository<Course> courseRepository, IMasterBaseRepository<Level> levelRepository, IMasterBaseRepository<Program> programRepository)
        {
            _studentRepository = studentRepository;
            _studentCompetitionEventRepository = studentCompetitionEventRepository;
            _courseResultRepository = courseResultRepository;
            _lessonResultRepository = lessonResultRepository;
            _courseRepository = courseRepository;
            _levelRepository = levelRepository;
            _programRepository = programRepository;
        }

        public async Task<MethodResult<IList<GoalProgressModel>>> Handle(GetLearningGoalResultQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<GoalProgressModel>>();

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
                              select new { sc.StudentId, cr.CourseResultId, c.LevelId, cr.Percent, c.ProgramId };

            if (request.ProgramIds != null && request.ProgramIds.Count > 0)
            {
                courseQuery = courseQuery.Where(p => request.ProgramIds.Contains(p.ProgramId));
            }

            if (request.LevelIds != null && request.LevelIds.Count > 0)
            {
                courseQuery = courseQuery.Where(p => request.LevelIds.Contains(p.LevelId));
            }

            var data = await courseQuery.ToListAsync(cancellationToken);

            var levels = await _levelRepository.Queryable.ToListAsync(cancellationToken);
            var programs = await _programRepository.Queryable.ToListAsync(cancellationToken);

            var result = data.GroupBy(x => x.LevelId)
                             .Select(levelGroup =>
                             {
                                 var level = levels.FirstOrDefault(p => p.LevelId == levelGroup.Key);
                                 var program = programs.FirstOrDefault(p => p.ProgramId == level?.ProgramId);
                                 return new GoalProgressModel
                                 {
                                     LevelId = levelGroup.Key,
                                     LevelName = level?.LevelCode ?? "Unknown",
                                     DisplayOrder = level?.LevelOrder ?? 0,
                                     ProgramName = program?.ProgramName,
                                     FarFromTarget = levelGroup.Count(x => !x.Percent.HasValue || x.Percent < 60),
                                     CloseToTarget = levelGroup.Count(x => x.Percent.HasValue && x.Percent >= 60 && x.Percent <= 74),
                                     OnTarget = levelGroup.Count(x => x.Percent.HasValue && x.Percent >= 75 && x.Percent <= 89),
                                     ExceededTarget = levelGroup.Count(x => x.Percent.HasValue && x.Percent >= 90 && x.Percent <= 100)
                                 };
                             })
                             .ToList();

            methodResult.Result = result.OrderBy(p => p.ProgramName).ThenBy(p => p.DisplayOrder).ToList();
            return methodResult;
        }
    }
}
