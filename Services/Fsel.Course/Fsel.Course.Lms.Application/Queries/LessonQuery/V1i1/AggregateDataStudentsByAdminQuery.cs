using Fsel.Common.ActionResults;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Course.Domain.Models.QueryModels.Lessons;
using Fsel.Course.Lms.Application.Services.ApplicationServices;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Lms.Application.Queries.LessonQuery.V1i1
{
    public class AggregateDataStudentsByAdminQuery : AggregateDataStudentsByAdminQueryModels, IRequest<MethodResult<AggregateDataStudentsByAdminModels>>
    {
    }

    public class AggregateDataStudentsByAdminQueryHandler : IRequestHandler<AggregateDataStudentsByAdminQuery, MethodResult<AggregateDataStudentsByAdminModels>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly ILearningService _learningService;
        private readonly ITestGroupResultRepository _testGroupResultRepository;

        public AggregateDataStudentsByAdminQueryHandler(ICourseRepository courseRepository, ICourseResultRepository courseResultRepository, ILearningService learningService, ITestGroupResultRepository testGroupResultRepository)
        {
            _courseRepository = courseRepository;
            _courseResultRepository = courseResultRepository;
            _learningService = learningService;
            _testGroupResultRepository = testGroupResultRepository;
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

            var testGroupResults = await _testGroupResultRepository.Queryable.WhereBulkContains(studentIds, p => p.StudentId).Where(p => p.TestType == EnumTestType.PlacementTest).ToListAsync(cancellationToken);

            testGroupResults = testGroupResults.OrderBy(p => p.CreatedDate).ToList();

            request.Students.Where(p => p.StudentId.HasValue).ForEach(p => students.Add(new AggregateDataStudentsByAdminModel
            {
                StudentId = p.StudentId!.Value,
                CourseId = p.CourseId,
                PTStatus = testGroupResults.FirstOrDefault(x => x.StudentId == p.StudentId)?.Status.ToString()
            }));

            if (coursesIds != null && coursesIds.Count > 0)
            {
                var courses = await _courseRepository.Queryable.Include(p => p.Program).ThenInclude(p => p.CategoryParent).Include(p => p.Level).WhereBulkContains(coursesIds, p => p.Id).ToListAsync(cancellationToken);

                var studentLearnIds = await _courseResultRepository.Queryable.WhereBulkContains(studentIds, p => p.StudentId).Select(x => x.StudentId).Distinct().ToListAsync(cancellationToken);

                var courseDict = courses.ToDictionary(x => x.Id);
                var studentLearnSet = studentLearnIds.ToHashSet();

                var learningComponentModels = request.Students.Where(p => p.StudentId.HasValue && p.CourseId.HasValue).Select(p => new GetLearningTreeFromCourseToTestModel()
                {
                    StudentId = p.StudentId ?? default,
                    CourseId = p.CourseId ?? default,
                }).ToList();

                var results = await _learningService.GetLearningTreeFromCourseToTest(learningComponentModels, cancellationToken);

                foreach (var student in students)
                {
                    var result = results.FirstOrDefault(p => p.StudentId == student.StudentId && p.LearningTemplateId == student.CourseId);
                    if (result != null)
                    {
                        var lessons = result.Children.SelectMany(p => p.Children).ToList();
                        student.TotalLesson = lessons.Count;
                        student.TotalLessonDone = lessons.Count(n => n.Status == EnumResultStatus.Done);
                    }

                    student.IsLearnStudent = studentLearnSet.Contains(student.StudentId);

                    if (student.CourseId.HasValue && courseDict.TryGetValue(student.CourseId.Value, out var course))
                    {
                        student.Program = course.Program?.Name;
                        student.Level = course.Level?.Name;
                        student.Level = course.Program?.CategoryParent?.Name;
                    }
                }
            }
            methodResult.Result = new AggregateDataStudentsByAdminModels()
            {
                Students = students
            };
            return methodResult;
        }
    }
}
