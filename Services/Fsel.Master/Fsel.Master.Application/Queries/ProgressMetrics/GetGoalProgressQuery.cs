// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Master.Application.Queries.ProgressMetrics
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Master.Domain.Entities;
    using Fsel.Master.Domain.IRepositories;
    using Fsel.Master.Domain.Models.Enums;
    using Fsel.Master.Domain.Models.QueryModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GoalProgressModel
    {
        public Guid LevelId { get; set; }

        public string? LevelName { get; set; }

        public string? ProgramName { get; set; }

        public int? DisplayOrder { get; set; }

        public int FarFromTarget { get; set; }

        public int CloseToTarget { get; set; }

        public int OnTarget { get; set; }

        public int ExceededTarget { get; set; }
    }

    public class GetGoalProgressQuery : BaseProgressMetricsQueryModel, IRequest<MethodResult<IList<GoalProgressModel>>>
    {
        public DateTime Date { get; set; }
    }

    public class GetGoalProgressQueryHandler : IRequestHandler<GetGoalProgressQuery, MethodResult<IList<GoalProgressModel>>>
    {
        private readonly IMasterBaseRepository<StudentProfileReport> _studentRepository;
        private readonly IMasterBaseRepository<StudentCompetitionEvent> _studentCompetitionEventRepository;
        private readonly IMasterBaseRepository<CourseResult> _courseResultRepository;
        private readonly IMasterBaseRepository<LessonResult> _lessonResultRepository;
        private readonly IMasterBaseRepository<Course> _courseRepository;
        private readonly IMasterBaseRepository<Level> _levelRepository;
        private readonly IMasterBaseRepository<Program> _programRepository;

        public GetGoalProgressQueryHandler(IMasterBaseRepository<StudentProfileReport> studentRepository, IMasterBaseRepository<StudentCompetitionEvent> studentCompetitionEventRepository, IMasterBaseRepository<CourseResult> courseResultRepository, IMasterBaseRepository<LessonResult> lessonResultRepository, IMasterBaseRepository<Course> courseRepository, IMasterBaseRepository<Level> levelRepository, IMasterBaseRepository<Program> programRepository)
        {
            _studentRepository = studentRepository;
            _studentCompetitionEventRepository = studentCompetitionEventRepository;
            _courseResultRepository = courseResultRepository;
            _lessonResultRepository = lessonResultRepository;
            _courseRepository = courseRepository;
            _levelRepository = levelRepository;
            _programRepository = programRepository;
        }

        public async Task<MethodResult<IList<GoalProgressModel>>> Handle(GetGoalProgressQuery request, CancellationToken cancellationToken)
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
                              select new { sc.StudentId, cr.CourseResultId, c.LevelId, cr.Status, c.ProgramId };

            if (request.ProgramIds != null && request.ProgramIds.Count > 0)
            {
                courseQuery = courseQuery.Where(p => p.ProgramId.HasValue && request.ProgramIds.Contains(p.ProgramId.Value));
            }

            if (request.LevelIds != null && request.LevelIds.Count > 0)
            {
                courseQuery = courseQuery.Where(p => p.LevelId.HasValue && request.LevelIds.Contains(p.LevelId.Value));
            }

            var time = Shared.Helpers.DateTimeHelper.GetWeekBoundaries(request.Date);
            var current = Shared.Helpers.DateTimeHelper.GetWeekBoundaries(DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam));

            var isPastWeek = time.SundayEnd < current.MondayStart;
            var mondayStart = time.MondayStart;
            var sundayEnd = time.SundayEnd;

            var lessonGrouped = _lessonResultRepository.Queryable
                                .Where(l => l.Status == EnumResultStatus.Done
                                            && l.CompletionDate >= mondayStart
                                            && l.CompletionDate <= sundayEnd)
                                .GroupBy(l => l.CourseResultId)
                                .Select(g => new
                                {
                                    CourseResultId = g.Key,
                                    CompletedCount = g.Count()
                                });

            var lessonQuery = from cq in courseQuery
                              join lg in lessonGrouped
                                  on cq.CourseResultId equals lg.CourseResultId into lgJoin
                              from lg in lgJoin.DefaultIfEmpty()
                              let completedCount = (int?)lg.CompletedCount ?? 0
                              select new
                              {
                                  cq.StudentId,
                                  cq.LevelId,
                                  CompletedLessons =
                                      (isPastWeek || cq.Status != EnumResultStatus.Done)
                                      ? completedCount
                                      : 3
                              };

            var progressData = from l in lessonQuery
                               select new
                               {
                                   l.StudentId,
                                   l.LevelId,
                                   l.CompletedLessons,
                                   TargetLessons = 3
                               };

            var data = await progressData.ToListAsync(cancellationToken);

            var levels = await _levelRepository.Queryable.ToListAsync(cancellationToken);
            var programs = await _programRepository.Queryable.ToListAsync(cancellationToken);

            var result = data.GroupBy(x => x.LevelId)
                             .Select(levelGroup =>
                             {
                                 var level = levels.FirstOrDefault(p => p.LevelId == levelGroup.Key);
                                 var program = programs.FirstOrDefault(p => p.ProgramId == level?.ProgramId);
                                 return new GoalProgressModel
                                 {
                                     LevelId = levelGroup.Key ?? default,
                                     LevelName = level?.LevelCode ?? "Unknown",
                                     DisplayOrder = level?.LevelOrder ?? 0,
                                     ProgramName = program?.ProgramName,
                                     FarFromTarget = levelGroup.Count(x => x.TargetLessons == 0 || x.CompletedLessons * 1.0 / x.TargetLessons < 0.5),
                                     CloseToTarget = levelGroup.Count(x => x.TargetLessons > 0
                                                                           && x.CompletedLessons * 1.0 / x.TargetLessons >= 0.5
                                                                           && x.CompletedLessons * 1.0 / x.TargetLessons < 1),
                                     OnTarget = levelGroup.Count(x => x.TargetLessons > 0 && x.CompletedLessons == x.TargetLessons),
                                     ExceededTarget = levelGroup.Count(x => x.TargetLessons > 0 && x.CompletedLessons > x.TargetLessons)
                                 };
                             })
                             .ToList();

            methodResult.Result = result.OrderBy(p => p.ProgramName).ThenBy(p => p.DisplayOrder).ToList();
            return methodResult;
        }
    }
}
