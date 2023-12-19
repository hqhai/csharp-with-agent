// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.CourseQuery
{
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.OrderServices;
    using Fsel.Course.Lms.Application.Services.OrderServices.Model;
    using Fsel.Course.Lms.Application.Services.TrainingServices;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Unit = Fsel.Course.Domain.Entities.Unit;

    public class GetCourseQuery : IRequest<MethodResult<CourseModel>>
    {
    }

    public class GetCourseQueryHandler : IRequestHandler<GetCourseQuery, MethodResult<CourseModel>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly ITrainingService _trainingService;
        private readonly AuthContext _authContext;
        private readonly IOrderService _orderService;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly IFinalTestResultRepository _finalTestResultRepository;

        public GetCourseQueryHandler(IMapper mapper,
            AuthContext authContext,
            IOrderService orderService,
            ICourseRepository courseRepository,
            IUserService userService,
            ITrainingService trainingService,
            IUnitResultRepository unitResultRepository,
            IMockTestResultRepository mockTestResultRepository,
            IFinalTestResultRepository finalTestResultRepository)
        {
            _courseRepository = courseRepository;
            _mapper = mapper;
            _userService = userService;
            _trainingService = trainingService;
            _authContext = authContext;
            _orderService = orderService;
            _unitResultRepository = unitResultRepository;
            _mockTestResultRepository = mockTestResultRepository;
            _finalTestResultRepository = finalTestResultRepository;
        }

        public async Task<MethodResult<CourseModel>> Handle(GetCourseQuery request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<CourseModel>();

            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }
            var student = studentResult?.Content?.Result;
            var studentId = student?.Id;
            var classResult = await _trainingService.GetClassByStudentId(studentId ?? default);
            if (!classResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallTrainingServiceError));
                return methodResult;
            }
            var @class = classResult?.Content?.Result;
            if (@class == null)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            var orderResult = await _orderService.GetStatusAsync(new GetStatusByUserCommandModel { CourseId = @class.CourseId, UserId = _authContext.CurrentUserId });
            if (!orderResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallOrderServiceError));
                return methodResult;
            }
            var status = orderResult?.Content?.Result ?? default;
            if (status != EnumOrderStatus.Payment)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var course = await _courseRepository.Queryable
                             .Include(x => x.CourseResults.Where(x => x.StudentId == studentId))
                             .Include(x => x.CourseUnitMockTests)
                             .FirstOrDefaultAsync(x => x.Id == @class.CourseId, cancellationToken);

            if (course == null)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            await UpdateCourse(course, student?.Id, cancellationToken).ConfigureAwait(false);

            course = await _courseRepository.GetIncludeCourseResult(course.Id, studentId);
            var courseModel = GetCourseModel(course, studentId);
            if (courseModel == null)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            courseModel.CourseClass = new CourseClassModel
            {
                Code = @class.Code,
                CourseId = course?.Id ?? default
            };
            var teachersResult = await _userService.GetTeacherByIdsAsync(new GetTeacherByIdsQueryModel { Ids = course?.CourseTeachers?.Select(x => x.TeacherId).ToList() });
            var teachers = teachersResult?.Content?.Result;
            if (teachersResult != null && teachersResult.IsSuccessStatusCode && teachers != null && courseModel.CourseTeachers != null)
            {
                foreach (var item in courseModel.CourseTeachers)
                {
                    var teacher = teachers.FirstOrDefault(x => x.Id == item.TeacherId);
                    item.FullName = teacher?.Human?.FullName;
                }
            }

            methodResult.Result = courseModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private CourseModel? GetCourseModel(Course? course, Guid? studentId)
        {
            if (course == null)
            {
                return null;
            }

            var courseModel = _mapper.Map<CourseModel>(course);
            courseModel.CourseUnitMockTests = course.CourseUnitMockTests.OrderBy(x => x!.DisplayOrder).ThenBy(x => x.CreatedDate).Select(x => new CourseUnitMockTestModel
            {
                DisplayOrder = x.DisplayOrder,
                CourseId = x.CourseId,
                FinalTestId = x.FinalTestId,
                MockTestId = x.MockTestId,
                UnitId = x.UnitId,
                FinalTest = GetFinalTest(x.FinalTest, studentId, course.Id),
                MockTest = GetMockTest(x.MockTest, studentId, course.Id),
                Unit = GetUnit(x.Unit, studentId, course.Id),
                Type = x.FinalTest != null ? nameof(x.FinalTest) : x.MockTest != null ? nameof(x.MockTest) : x.Unit != null ? nameof(x.Unit) : null
            }).ToList();
            courseModel.CourseTeachers = _mapper.Map<List<CourseTeacherModel>>(course.CourseTeachers);
            courseModel.CourseResult = _mapper.Map<CourseResultModel>(course.CourseResults.FirstOrDefault(x => x.StudentId == studentId));
            return courseModel;
        }

        public async Task UpdateCourse(Course? course, Guid? studentId, CancellationToken cancellationToken)
        {
            if (course == null)
            {
                return;
            }

            if (!course.CourseResults.Any())
            {
                course.CourseResults.Add(new CourseResult
                {
                    StudentId = studentId ?? default,
                    Status = EnumResultStatus.New
                });
            }

            var courseUnitMockTests = course.CourseUnitMockTests.OrderBy(x => x.DisplayOrder).ToList();
            var checkUnitResultAll = _unitResultRepository.Queryable.Where(x => x.StudentId == studentId).ToList();
            var checkMockTestResultAll = _mockTestResultRepository.Queryable.Where(x => x.StudentId == studentId).ToList();
            var checkFinalResultAll = _finalTestResultRepository.Queryable.Where(x => x.StudentId == studentId).ToList();

            foreach (var courseUnitMockTest in courseUnitMockTests)
            {
                var index = courseUnitMockTests.IndexOf(courseUnitMockTest);
                var checkUnitResult = checkUnitResultAll.Any(x => x.UnitId == courseUnitMockTest.UnitId && x.CourseId == courseUnitMockTest.CourseId);
                var checkFinalResult = checkFinalResultAll.Any(x => x.FinalTestId == courseUnitMockTest.FinalTestId && x.CourseId == courseUnitMockTest.CourseId);
                var checkMockTest = checkMockTestResultAll.Any(x => x.MockTestId == courseUnitMockTest.MockTestId && x.CourseId == courseUnitMockTest.CourseId);

                if (!courseUnitMockTest.UnitId.HasValue && !courseUnitMockTest.FinalTestId.HasValue && !courseUnitMockTest.MockTestId.HasValue)
                {
                    break;
                }

                if (!checkUnitResult && courseUnitMockTest.UnitId.HasValue)
                {
                    AddUnit(index, checkUnitResultAll, checkMockTestResultAll, courseUnitMockTests, course, courseUnitMockTest, studentId);
                    continue;
                }

                if (!checkFinalResult && courseUnitMockTest.FinalTestId.HasValue)
                {
                    AddFinal(index, checkUnitResultAll, courseUnitMockTests, course, courseUnitMockTest, studentId);
                    continue;
                }

                if (!checkMockTest && courseUnitMockTest.MockTestId.HasValue)
                {
                    AddMockTest(index, checkUnitResultAll, courseUnitMockTests, course, courseUnitMockTest, studentId);
                    continue;
                }
            }
            course = _courseRepository.Update(course);
            await _courseRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
        }

        public static void AddUnit(int index, List<UnitResult> checkUnitResultAll, List<MockTestResult> checkMockTestResultAll, List<CourseUnitMockTest> courseUnitMockTests, Course? course, CourseUnitMockTest courseUnitMockTest, Guid? studentId)
        {
            var courseUnitMockTestFirst = index != 0 ? courseUnitMockTests[index - 1] : new CourseUnitMockTest();
            // index !=0 ktra unit trc nó có trong result với status = done thì add result mới với Status new
            bool checkFirstDone = false;
            if (courseUnitMockTestFirst.UnitId.HasValue && index != 0)
            {
                checkFirstDone = checkUnitResultAll.Any(x => x.UnitId == courseUnitMockTestFirst.UnitId && x.CourseId == courseUnitMockTestFirst.CourseId && x.Status == EnumResultStatus.Done);
            }
            else if (courseUnitMockTestFirst.MockTestId.HasValue && index != 0)
            {
                checkFirstDone = checkMockTestResultAll.Any(x => x.MockTestId == courseUnitMockTestFirst.MockTestId && x.CourseId == courseUnitMockTestFirst.CourseId && x.Status == EnumResultStatus.Done);
            }
            course.UnitResults.Add(new UnitResult
            {
                UnitId = courseUnitMockTest != null ? courseUnitMockTest.UnitId!.Value : default,
                StudentId = studentId ?? default,
                Status = (index == 0 || checkFirstDone) ? EnumResultStatus.New : EnumResultStatus.Unfinished
            });
        }

        public static void AddMockTest(int index, List<UnitResult> checkUnitResultAll, List<CourseUnitMockTest> courseUnitMockTests, Course? course, CourseUnitMockTest courseUnitMockTest, Guid? studentId)
        {
            var courseUnitMockTestFirt = (index != 0) ? courseUnitMockTests[index - 1] : new CourseUnitMockTest();
            var checkFirstDone = checkUnitResultAll.Any(x => x.UnitId == courseUnitMockTestFirt.UnitId && x.CourseId == courseUnitMockTestFirt.CourseId && x.Status == EnumResultStatus.Done);
            course.MockTestResults.Add(new MockTestResult
            {
                MockTestId = courseUnitMockTest != null ? courseUnitMockTest.MockTestId!.Value : default,
                StudentId = studentId ?? default,
                Status = checkFirstDone ? EnumResultStatus.New : EnumResultStatus.Unfinished
            });
        }

        public static void AddFinal(int index, List<UnitResult> checkUnitResultAll, List<CourseUnitMockTest> courseUnitMockTests, Course? course, CourseUnitMockTest courseUnitMockTest, Guid? studentId)
        {
            var courseUnitMockTestFirt = index != 0 ? courseUnitMockTests[index - 1] : new CourseUnitMockTest();
            var checkFirstDone = checkUnitResultAll.Any(x => x.UnitId == courseUnitMockTestFirt.UnitId && x.CourseId == courseUnitMockTestFirt.CourseId && x.Status == EnumResultStatus.Done);
            course?.FinalTestResults.Add(new FinalTestResult
            {
                FinalTestId = courseUnitMockTest != null ? courseUnitMockTest.FinalTestId!.Value : default,
                StudentId = studentId ?? default,
                Status = checkFirstDone ? EnumResultStatus.New : EnumResultStatus.Unfinished
            });
        }

        public UnitModel? GetUnit(Unit? unit, Guid? studentId, Guid courseId)
        {
            if (unit == null)
            {
                return null;
            }
            var unitModel = _mapper.Map<UnitModel>(unit);
            unitModel.UnitResult = _mapper.Map<UnitResultModel>(unit.UnitResults.FirstOrDefault(y => y.StudentId == studentId && y.CourseId == courseId));
            return unitModel;
        }

        public MockTestModel? GetMockTest(MockTest? mockTest, Guid? studentId, Guid courseId)
        {
            if (mockTest == null)
            {
                return null;
            }
            var mockTestModel = _mapper.Map<MockTestModel>(mockTest);
            mockTestModel.MockTestResult = _mapper.Map<MockTestResultModel>(mockTest.MockTestResults.FirstOrDefault(y => y.StudentId == studentId && y.CourseId == courseId));
            return mockTestModel;
        }

        public FinalTestModel? GetFinalTest(FinalTest? finalTest, Guid? studentId, Guid courseId)
        {
            if (finalTest == null)
            {
                return null;
            }
            var finalTestModel = _mapper.Map<FinalTestModel>(finalTest);
            finalTestModel.FinalTestResult = _mapper.Map<FinalTestResultModel>(finalTest.FinalTestResults.FirstOrDefault(y => y.StudentId == studentId && y.CourseId == courseId));
            return finalTestModel;
        }
    }
}
