// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.CourseQuery
{
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Course.Lms.Application.Services.OrderServices;
    using Fsel.Course.Lms.Application.Services.TrainingServices;
    using Fsel.Course.Lms.Application.Services.TrainingServices.Models;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    public class GetCourseQuery : IRequest<MethodResult<CourseModel>>
    {
        public Guid? UserId { get; set; }
    }

    public class GetCourseQueryHandler : IRequestHandler<GetCourseQuery, MethodResult<CourseModel>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IUserService _userService;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly IMapper _mapper;
        private readonly IUnitRepository _unitRepository;
        private readonly IMockTestRepository _mockTestRepository;
        private readonly IFinalTestRepository _finalTestRepository;
        private readonly ITrainingService _trainingService;
        private readonly AuthContext _authContext;
        private readonly IOrderService _orderService;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly SaveUserCourseSettingPublisher _saveUserCourseSettingPublisher;
        private readonly IFinalTestResultRepository _finalTestResultRepository;
        private readonly ILogger<object> _logger;

        public GetCourseQueryHandler(
            AuthContext authContext,
            IOrderService orderService,
            ICourseRepository courseRepository,
            IUserService userService,
            ICourseResultRepository courseResultRepository,
            IMapper mapper,
            IUnitRepository unitRepository,
            IMockTestRepository mockTestRepository,
            IFinalTestRepository finalTestRepository,
            ITrainingService trainingService,
            IUnitResultRepository unitResultRepository,
            IMockTestResultRepository mockTestResultRepository,
            SaveUserCourseSettingPublisher saveUserCourseSettingPublisher,
            IFinalTestResultRepository finalTestResultRepository,
            ILogger<GetCourseQueryHandler> logger)
        {
            _courseRepository = courseRepository;
            _userService = userService;
            _courseResultRepository = courseResultRepository;
            _mapper = mapper;
            _unitRepository = unitRepository;
            _mockTestRepository = mockTestRepository;
            _finalTestRepository = finalTestRepository;
            _trainingService = trainingService;
            _authContext = authContext;
            _orderService = orderService;
            _unitResultRepository = unitResultRepository;
            _mockTestResultRepository = mockTestResultRepository;
            _saveUserCourseSettingPublisher = saveUserCourseSettingPublisher;
            _finalTestResultRepository = finalTestResultRepository;
            _logger = logger;
        }

        public async Task<MethodResult<CourseModel>> Handle(GetCourseQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<CourseModel>();

            var method = await Validate(request);
            if (!method.IsOK)
            {
                methodResult.AddErrorBadRequest(method.ErrorMessages);
                return methodResult;
            }

            var (student, @class) = method.Result;
            if (@class == null)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            var courseResult = await _courseResultRepository.Queryable.FirstOrDefaultAsync(x => x.StudentId == student.Id && x.WorkingStatus == EnumWorkingStatus.Active, cancellationToken);
            var course = await _courseRepository.Queryable
                 .Include(x => x.CourseResults.Where(x => courseResult != null && x.Id == courseResult.Id))
                 .Include(x => x.CourseUnitMockTests).AsNoTracking()
                 .FirstOrDefaultAsync(x => courseResult != null && x.Id == courseResult.CourseId, cancellationToken);

            if (course == null)
            {
                course = await _courseRepository.Queryable
                         .Include(x => x.CourseResults.Where(x => x.StudentId == student.Id && x.CourseId == @class.CourseId && x.WorkingStatus == EnumWorkingStatus.Active))
                         .Include(x => x.CourseUnitMockTests)
                         .AsNoTracking()
                         .FirstOrDefaultAsync(x => x.Id == @class.CourseId, cancellationToken);
            }

            if (course == null)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            await SaveCourseSettingAsync(course, request.UserId ?? _authContext.CurrentUserId, cancellationToken);
            await UpdateCourse(course, student.Id, cancellationToken);

            var courseModel = await GetCourseAsync(course.Id, student.Id, @class.Code);
            if (courseModel == null)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var teachersResult = await _userService.GetTeacherByIdsAsync(new GetTeacherByIdsQueryModel { Ids = courseModel.CourseTeachers?.Select(x => x.TeacherId).ToList() });
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

        private async Task<MethodResult<(StudentModel, ClassModel)>> Validate(GetCourseQuery request)
        {
            var methodResult = new MethodResult<(StudentModel, ClassModel)>();
            var userId = request.UserId ?? _authContext.CurrentUserId;

            var studentResult = await _userService.GetStudentByUserIdAsync(userId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }
            var student = studentResult?.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentResult));
                return methodResult;
            }
            var classResult = await _trainingService.GetClassToStudentIdAsync(student.Id);
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
            if (!student.ExpiredDate.HasValue || student.ExpiredDate.Value.Date < DateTime.UtcNow.Date)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            methodResult.Result = (student, @class);
            return methodResult;
        }

        private async Task SaveCourseSettingAsync(Course course, Guid? userId, CancellationToken cancellationToken)
        {
            var userCourseSettingResults = await _userService.GetUserCourseSettingsAsync(userId ?? _authContext.CurrentUserId);
            if (!userCourseSettingResults.IsSuccessStatusCode)
            {
                return;
            }
            var userCourseSetting = userCourseSettingResults.Content?.Result?.FirstOrDefault(x => x.CourseLevel == course.CourseLevel);
            if (userCourseSetting != null)
            {
                return;
            }
            await _saveUserCourseSettingPublisher.Publish(new SaveUserCourseSettingQueueModel
            {
                CourseLevel = course.CourseLevel,
                Type = EnumUserCourseType.ResetAndLearnAgain,
                UserId = userId ?? _authContext.CurrentUserId
            }, cancellationToken).ConfigureAwait(false);
        }

        private async Task<CourseModel?> GetCourseAsync(Guid id, Guid? studentId, string? classCode)
        {
            var course = await _courseRepository.Queryable
                          .Include(x => x.CourseUnitMockTests.OrderBy(x => x.DisplayOrder).ThenBy(x => x.CreatedDate))
                          .Include(x => x.CourseTeachers)
                          .Include(x => x.CourseResults.Where(x => x.StudentId == studentId && x.WorkingStatus == EnumWorkingStatus.Active))
                          .Where(x => x.Id == id)
                          .AsNoTracking()
                          .FirstOrDefaultAsync();
            if (course == null)
            {
                return default;
            }

            var courseDto = _mapper.Map<CourseModel>(course);
            courseDto.CourseClass = new CourseClassModel
            {
                Code = classCode,
                CourseId = courseDto.Id
            };
            courseDto.CourseResult = _mapper.Map<CourseResultModel>(course.CourseResults.FirstOrDefault(x => x.StudentId == studentId));
            courseDto.CourseTeachers = _mapper.Map<IList<CourseTeacherModel>>(course.CourseTeachers.OrderBy(x => x.CreatedDate));
            courseDto.CourseUnitMockTests = await GetCourseUnitMockTestsAsync(course.CourseUnitMockTests.ToList(), course, studentId);
            return courseDto;
        }

        private async Task<IList<CourseUnitMockTestModel>> GetCourseUnitMockTestsAsync(IList<CourseUnitMockTest> courseUnitMockTests, Course course, Guid? studentId)
        {
            var courseUnitMockTestDtos = new List<CourseUnitMockTestModel>();
            var unitIds = courseUnitMockTests.Where(x => x.UnitId.HasValue).Select(x => x.UnitId!.Value).ToList();
            var units = await GetUnits(unitIds, course.Id, studentId, course.CourseType);
            var mockTests = new List<MockTestModel>();

            if (course.CourseType == EnumCourseType.Ielts)
            {
                var mockTestIds = courseUnitMockTests.Where(x => x.MockTestId.HasValue).Select(x => x.MockTestId!.Value).ToList();
                mockTests = await GetMockTests(mockTestIds, course, studentId);
            }

            foreach (var courseUnitMockTest in courseUnitMockTests)
            {
                var courseUnitMockTestDto = _mapper.Map<CourseUnitMockTestModel>(courseUnitMockTest);
                if (courseUnitMockTestDto.UnitId.HasValue)
                {
                    courseUnitMockTestDto.Unit = units.FirstOrDefault(x => x.Id == courseUnitMockTestDto.UnitId.Value);
                }
                else if (courseUnitMockTestDto.MockTestId.HasValue && mockTests.Any())
                {
                    courseUnitMockTestDto.MockTest = mockTests.FirstOrDefault(x => x.Id == courseUnitMockTestDto.MockTestId.Value);
                }
                else if (courseUnitMockTestDto.FinalTestId.HasValue)
                {
                    courseUnitMockTestDto.FinalTest = await GetFinalTest(courseUnitMockTest, studentId);
                }
                courseUnitMockTestDtos.Add(courseUnitMockTestDto);
            }
            return courseUnitMockTestDtos.OrderBy(x => x.DisplayOrder).ToList();
        }

        private async Task<IList<UnitModel>> GetUnits(IList<Guid> unitIds, Guid courseId, Guid? studentId, EnumCourseType courseType)
        {
            var query = _unitRepository.Queryable;
            if (courseType == EnumCourseType.Ielts)
            {
                query = query.Include(x => x.UnitResults.Where(x => x.CourseId == courseId && x.StudentId == studentId))
                            .Include(x => x.LessonResults.Where(x => x.StudentId == studentId && x.CourseId == courseId))
                            .Include(x => x.UnitLessons)
                            .Include(x => x.UnitSkillMockTests)
                                .ThenInclude(x => x.MockTest)
                                .ThenInclude(x => x!.MockTestResults.Where(x => x.StudentId == studentId && x.CourseId == courseId));
            }
            else
            {
                query = query.Include(x => x.UnitResults.Where(x => x.CourseId == courseId && x.StudentId == studentId))
                            .Include(x => x.LessonResults.Where(x => x.StudentId == studentId && x.CourseId == courseId))
                            .Include(x => x.UnitLessons);
            }
            var units = await query.Where(x => unitIds.Contains(x.Id))
                             .AsNoTracking()
                             .ToListAsync();
            return units.Select(x =>
            {
                var unit = _mapper.Map<UnitModel>(x);
                unit.UnitResult = _mapper.Map<UnitResultModel>(x.UnitResults.FirstOrDefault());
                return unit;
            }).ToList();
        }

        private async Task<List<MockTestModel>> GetMockTests(IList<Guid> mockTestIds, Course course, Guid? studentId)
        {
            var mockTests = await _mockTestRepository.Queryable.Include(x => x.MockTestResults.Where(x => x.CourseId == course.Id && mockTestIds.Contains(x.MockTestId) && x.StudentId == studentId))
                                                                    .ThenInclude(x => x.SectionGroupResults.Where(x => x.StudentId == studentId))
                                                                .Include(x => x.MockTestSections)
                                                                .Where(y => mockTestIds.Contains(y.Id))
                                                                .AsNoTracking()
                                                                .ToListAsync();

            return mockTests.Select(x =>
            {
                var mockTest = _mapper.Map<MockTestModel>(x);
                mockTest.MockTestResult = _mapper.Map<MockTestResultModel>(x.MockTestResults.FirstOrDefault());
                mockTest.MockTestResult.TargetBandScore = course.CourseLevel.GetBandScore();
                return mockTest;
            }).ToList();
        }

        private async Task<FinalTestModel?> GetFinalTest(CourseUnitMockTest courseUnitMockTest, Guid? studentId)
        {
            var finalTests = await _finalTestRepository.Queryable.Include(x => x.FinalTestResults.Where(x => x.CourseId == courseUnitMockTest.CourseId && x.StudentId == studentId))
                                                                    .ThenInclude(x => x.SectionGroupResults.Where(x => x.StudentId == studentId))
                                                                .Include(x => x.FinalTestSections)
                                                                .Where(y => y.Id == courseUnitMockTest.FinalTestId)
                                                                .AsNoTracking()
                                                                .ToListAsync();
            return finalTests.Select(x =>
            {
                var finalTest = _mapper.Map<FinalTestModel>(x);
                finalTest.FinalTestResult = _mapper.Map<FinalTestResultModel>(x.FinalTestResults.FirstOrDefault());
                return finalTest;
            }).FirstOrDefault();
        }

        public async Task UpdateCourse(Course? course, Guid? studentId, CancellationToken cancellationToken)
        {
            if (course == null)
            {
                return;
            }

            if (!course.CourseResults.Any())
            {
                var courseResult = new CourseResult
                {
                    StudentId = studentId ?? default,
                    Status = EnumResultStatus.New,
                    CourseId = course.Id
                };
                await _courseResultRepository.BulkMergeAsync(new List<CourseResult> { courseResult }, bulk =>
                {
                    bulk.ColumnPrimaryKeyExpression = c => new { c.CourseId, c.StudentId, c.IsDeleted };
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
                    continue;
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
            if (course.UnitResults.Any())
            {
                var unitResults = course.UnitResults.ToList();
                await _unitResultRepository.BulkMergeAsync(unitResults, bulk =>
                {
                    bulk.ColumnPrimaryKeyExpression = entity => new { entity.UnitId, entity.CourseId, entity.StudentId, entity.IsDeleted };
                });
            }
            if (course.FinalTestResults.Any())
            {
                var finalTestResults = course.FinalTestResults.ToList();
                await _finalTestResultRepository.BulkMergeAsync(finalTestResults, bulk =>
                {
                    bulk.ColumnPrimaryKeyExpression = entity => new { entity.FinalTestId, entity.CourseId, entity.StudentId, entity.IsDeleted };
                });
            }
            if (course.MockTestResults.Any())
            {
                var mockTestResults = course.MockTestResults.ToList();
                await _mockTestResultRepository.BulkMergeAsync(mockTestResults, bulk =>
                {
                    bulk.ColumnPrimaryKeyExpression = entity => new { entity.CourseId, entity.StudentId, entity.MockTestId, entity.IsDeleted };
                });
            }
        }

        public static void AddUnit(int index, IList<UnitResult>? checkUnitResultAll, IList<MockTestResult>? checkMockTestResultAll, IList<CourseUnitMockTest>? courseUnitMockTests, Course course, CourseUnitMockTest courseUnitMockTest, Guid? studentId)
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
                CourseId = course.Id,
                Status = (index == 0 || checkFirstDone) ? EnumResultStatus.New : EnumResultStatus.Unfinished
            });
        }

        public static void AddMockTest(int index, IList<UnitResult>? checkUnitResultAll, IList<CourseUnitMockTest>? courseUnitMockTests, Course course, CourseUnitMockTest courseUnitMockTest, Guid? studentId)
        {
            var courseUnitMockTestFirt = (index != 0) ? courseUnitMockTests[index - 1] : new CourseUnitMockTest();
            var checkFirstDone = checkUnitResultAll.Any(x => x.UnitId == courseUnitMockTestFirt.UnitId && x.CourseId == courseUnitMockTestFirt.CourseId && x.Status == EnumResultStatus.Done);
            course.MockTestResults.Add(new MockTestResult
            {
                MockTestId = courseUnitMockTest != null ? courseUnitMockTest.MockTestId!.Value : default,
                StudentId = studentId ?? default,
                CourseId = course.Id,
                Status = checkFirstDone ? EnumResultStatus.New : EnumResultStatus.Unfinished
            });
        }

        public static void AddFinal(int index, IList<UnitResult>? checkUnitResultAll, IList<CourseUnitMockTest>? courseUnitMockTests, Course? course, CourseUnitMockTest courseUnitMockTest, Guid? studentId)
        {
            var courseUnitMockTestFirt = index != 0 ? courseUnitMockTests[index - 1] : new CourseUnitMockTest();
            var checkFirstDone = checkUnitResultAll.Any(x => x.UnitId == courseUnitMockTestFirt.UnitId && x.CourseId == courseUnitMockTestFirt.CourseId && x.Status == EnumResultStatus.Done);
            course?.FinalTestResults.Add(new FinalTestResult
            {
                FinalTestId = courseUnitMockTest != null ? courseUnitMockTest.FinalTestId!.Value : default,
                StudentId = studentId ?? default,
                CourseId = course.Id,
                Status = checkFirstDone ? EnumResultStatus.New : EnumResultStatus.Unfinished
            });
        }
    }
}
