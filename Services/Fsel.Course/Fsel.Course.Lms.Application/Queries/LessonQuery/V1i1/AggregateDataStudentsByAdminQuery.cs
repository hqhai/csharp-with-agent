using Fsel.Common.ActionResults;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Course.Domain.Models.QueryModels.Lessons;
using Fsel.Course.Lms.Application.Services.ApplicationServices;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Fsel.Course.Lms.Application.Queries.LessonQuery.V1i1
{
    public class AggregateDataStudentsByAdminQuery : AggregateDataStudentsByAdminQueryModels, IRequest<MethodResult<AggregateDataStudentsByAdminModels>>
    {
    }

    public class AggregateDataStudentsByAdminQueryHandler : IRequestHandler<AggregateDataStudentsByAdminQuery, MethodResult<AggregateDataStudentsByAdminModels>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly ICourseUnitMockTestRepository _courseUnitMockTestRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly IUnitLessonRepository _unitLessonRepository;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly IPlacementTestGroupResultRepository _placementTestGroupResultRepository;
        private readonly IAggregateResultQueryService _aggregateResultQueryService;
        private readonly IServiceProvider _serviceProvider;

        public AggregateDataStudentsByAdminQueryHandler(ICourseRepository courseRepository, ILessonResultRepository lessonResultRepository, ICourseUnitMockTestRepository courseUnitMockTestRepository, IUnitRepository unitRepository, IUnitLessonRepository unitLessonRepository, ICourseResultRepository courseResultRepository, IPlacementTestGroupResultRepository placementTestGroupResultRepository, IAggregateResultQueryService aggregateResultQueryService, IServiceProvider serviceProvider)
        {
            _courseRepository = courseRepository;
            _lessonResultRepository = lessonResultRepository;
            _courseUnitMockTestRepository = courseUnitMockTestRepository;
            _unitRepository = unitRepository;
            _unitLessonRepository = unitLessonRepository;
            _courseResultRepository = courseResultRepository;
            _placementTestGroupResultRepository = placementTestGroupResultRepository;
            _aggregateResultQueryService = aggregateResultQueryService;
            _serviceProvider = serviceProvider;
        }

        public async Task<MethodResult<AggregateDataStudentsByAdminModels>> Handle(AggregateDataStudentsByAdminQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<AggregateDataStudentsByAdminModels>();

            var coursesIds = request.Students?.Where(p => p.CourseId.HasValue).Select(p => p.CourseId).Distinct().ToList();
            var studentIds = request.Students?.Where(p => p.StudentId.HasValue).Select(p => p.StudentId).Distinct().ToList();
            if (request.Students == null || !request.Students.Any())
            {
                return methodResult;
            }

            var students = new List<AggregateDataStudentsByAdminModel>();

            var placeTestGroupResults = await _placementTestGroupResultRepository.Queryable.WhereBulkContains(studentIds, p => p.StudentId).ToListAsync(cancellationToken);
            placeTestGroupResults = placeTestGroupResults.OrderByDescending(p => p.CreatedDate).ToList();

            request.Students.Where(p => p.StudentId.HasValue).ForEach(p => students.Add(new AggregateDataStudentsByAdminModel
            {
                StudentId = p.StudentId!.Value,
                CourseId = p.CourseId,
                PTStatus = placeTestGroupResults.FirstOrDefault(x => x.StudentId == p.StudentId)?.Status.ToString()
            }));

            if (coursesIds != null && coursesIds.Count > 0)
            {
                var courses = await _courseRepository.Queryable.Include(p => p.Program).Include(p => p.Level).WhereBulkContains(coursesIds, p => p.Id).ToListAsync(cancellationToken);

                var lessonModels = await (from c in _courseRepository.Queryable.WhereBulkContains(coursesIds, p => p.Id)
                                          join cumt in _courseUnitMockTestRepository.Queryable on c.Id equals cumt.CourseId
                                          join u in _unitRepository.Queryable on cumt.UnitId equals u.Id
                                          join ul in _unitLessonRepository.Queryable on u.Id equals ul.UnitId
                                          select new
                                          {
                                              CourseId = c.Id,
                                              ul.LessonId
                                          }).ToListAsync(cancellationToken);

                var lessonResultModels = await (from cr in _courseResultRepository.Queryable.WhereBulkContains(studentIds, p => p.StudentId)
                                                join lr in _lessonResultRepository.Queryable on cr.Id equals lr.CourseResultId
                                                where cr.WorkingStatus == Shared.Enums.EnumWorkingStatus.Active && lr.Status == EnumResultStatus.Done
                                                select new
                                                {
                                                    StudentId = cr.StudentId,
                                                    CourseId = cr.CourseId,
                                                    LessonResultId = lr.Id,
                                                    CourseResultId = cr.Id
                                                }).ToListAsync(cancellationToken);
                var studentLearnIds = await _courseResultRepository.Queryable.WhereBulkContains(studentIds, p => p.StudentId).Select(x => x.StudentId).Distinct().ToListAsync(cancellationToken);

                var courseDict = courses.ToDictionary(x => x.Id);
                var studentLearnSet = studentLearnIds.ToHashSet();

                await Parallel.ForEachAsync(
                    students,
                    new ParallelOptions
                    {
                        MaxDegreeOfParallelism = 10,
                        CancellationToken = cancellationToken
                    },
                    async (p, ct) =>
                    {
                        var lessonResult = lessonResultModels.FirstOrDefault(x => x.StudentId == p.StudentId);

                        if (lessonResult != null)
                        {
                            using (var scope = _serviceProvider.CreateScope())
                            {
                                var aggregateResultQueryService = scope.ServiceProvider.GetRequiredService<IAggregateResultQueryService>();

                                var learningTree = await aggregateResultQueryService.GetLearningTreeFromCourseToLesson(
                                            p.StudentId,
                                            lessonResult.CourseResultId,
                                            ct);

                                var lessons = learningTree
                                    .GetAllItemByType<LessonComponent>()
                                    .ToList();

                                p.TotalLesson = lessons.Count;
                                p.TotalLessonDone = lessons.Count(n => n.Status == EnumResultStatus.Done);
                            }
                        }

                        p.IsLearnStudent = studentLearnSet.Contains(p.StudentId);

                        if (p.CourseId.HasValue && courseDict.TryGetValue(p.CourseId.Value, out var course))
                        {
                            p.Program = course.Program?.Name;
                            p.Level = course.Level?.Name;
                        }
                    });
            }
            methodResult.Result = new AggregateDataStudentsByAdminModels()
            {
                Students = students
            };
            return methodResult;
        }
    }
}
