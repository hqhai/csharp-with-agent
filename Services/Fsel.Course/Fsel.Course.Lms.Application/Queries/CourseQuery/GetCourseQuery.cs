// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.CourseQuery
{
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
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

        public GetCourseQueryHandler(IMapper mapper,
            AuthContext authContext,
            IOrderService orderService,
            ICourseRepository courseRepository,
            IUserService userService,
            ITrainingService trainingService)
        {
            _courseRepository = courseRepository;
            _mapper = mapper;
            _userService = userService;
            _trainingService = trainingService;
            _authContext = authContext;
            _orderService = orderService;
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
                methodResult.Result = default;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            var orderResult = await _orderService.IsCheckStatusUser(new IsCheckPaymentStatusByUserModel { ClassId = @class.Id, CourseId = @class.CourseId, PackageId = @class.PackageId, UserId = _authContext.CurrentUserId });
            if (!orderResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallOrderServiceError));
                return methodResult;
            }
            var isCheckUserOrder = orderResult?.Content?.Result ?? default;
            if (!isCheckUserOrder)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(isCheckUserOrder));
                return methodResult;
            }
            var course = await _courseRepository.Queryable
                             .Include(x => x.CourseResults)
                             .Include(x => x.CourseUnitMockTests)
                             .FirstOrDefaultAsync(x => x.Id == @class.CourseId, cancellationToken);

            if (course == null)
            {
                methodResult.Result = default;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            if (course.CourseResults.FirstOrDefault(x => x.CourseId == course.Id && x.StudentId == studentId) == null)
            {
                await UpdateCourse(course, student?.Id, cancellationToken).ConfigureAwait(false);
            }
            course = await _courseRepository.GetIncludeCourseResult(course.Id, studentId);
            var courseModel = GetCourseModel(course, studentId);
            if (courseModel == null)
            {
                methodResult.Result = null;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
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
            return new CourseModel
            {
                Id = course.Id,
                Name = course.Name,
                Code = course.Code,
                Status = course.Status,
                CourseType = course.CourseType,
                CreatedDate = course.CreatedDate,
                InstructionContent = course.InstructionContent,
                CourseLevel = course.CourseLevel,
                CourseUnitMockTests = course.CourseUnitMockTests.OrderBy(x => x!.DisplayOrder).Select(x => new CourseUnitMockTestModel
                {
                    DisplayOrder = x.DisplayOrder,
                    CourseId = x.CourseId,
                    FinalTestId = x.FinalTestId,
                    MockTestId = x.MockTestId,
                    UnitId = x.UnitId,
                    FinalTest = GetFinalTest(x.FinalTest, studentId, course.Id),
                    MockTest = GetMockTest(x.MockTest, studentId, course.Id),
                    Unit = GetUnit(x.Unit, studentId, course.Id)
                }).ToList(),
                CourseTeachers = _mapper.Map<List<CourseTeacherModel>>(course.CourseTeachers),
                CourseResult = _mapper.Map<CourseResultModel>(course.CourseResults.FirstOrDefault(x => x.StudentId == studentId)),
            };
        }

        public async Task UpdateCourse(Course? course, Guid? studentId, CancellationToken cancellationToken)
        {
            if (course == null)
            {
                return;
            }
            course.CourseResults.Add(new CourseResult
            {
                StudentId = studentId ?? default,
                Status = EnumCourseStatus.Active
            });
            var unitIds = course.CourseUnitMockTests.Where(x => x.UnitId != null).OrderBy(x => x.DisplayOrder).Select(x => x.UnitId).ToList();
            var finalTestIds = course.CourseUnitMockTests.Where(x => x.FinalTestId != null).Select(x => x.FinalTestId).ToList();
            var mockTestIds = course.CourseUnitMockTests.Where(x => x.MockTestId != null).Select(x => x.MockTestId).ToList();
            if (unitIds.Count > 0)
            {
                course.UnitResults = unitIds.Select((x, index) => new UnitResult
                {
                    UnitId = x ?? default,
                    StudentId = studentId ?? default,
                    Status = index == 0 ? EnumResultStatus.New : EnumResultStatus.Unfinished
                }).ToList();
            }
            if (finalTestIds.Count > 0)
            {
                course.FinalTestResults = finalTestIds.Select(x => new FinalTestResult
                {
                    FinalTestId = x ?? default,
                    StudentId = studentId ?? default,
                    Status = EnumResultStatus.Unfinished
                }).ToList();
            }
            if (mockTestIds.Count > 0)
            {
                course.MockTestResults = mockTestIds.Select(x => new MockTestResult
                {
                    MockTestId = x ?? default,
                    StudentId = studentId ?? default,
                    Status = EnumResultStatus.Unfinished
                }).ToList();
            }
            course = _courseRepository.Update(course);
            await _courseRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
        }

        public UnitModel? GetUnit(Unit? unit, Guid? studentId, Guid courseId)
        {
            if (unit == null)
            {
                return null;
            }
            return new UnitModel
            {
                Id = unit.Id,
                Name = unit.Name,
                Code = unit.Code,
                CourseLevel = unit.CourseLevel,
                CreatedDate = unit.CreatedDate,
                CreatedFullName = unit.CreatedFullName,
                CreatedUserId = unit.CreatedUserId,
                IsActive = unit.CourseUnitMockTests.Any(),
                UnitResult = _mapper.Map<UnitResultModel>(unit.UnitResults.FirstOrDefault(y => y.StudentId == studentId && y.CourseId == courseId))
            };
        }

        public MockTestModel? GetMockTest(MockTest? mockTest, Guid? studentId, Guid courseId)
        {
            if (mockTest == null)
            {
                return null;
            }
            return new MockTestModel
            {
                Id = mockTest.Id,
                Name = mockTest.Name,
                MockTestType = mockTest.MockTestType,
                CreatedDate = mockTest.CreatedDate,
                CreatedFullName = mockTest.CreatedFullName,
                CreatedUserId = mockTest.CreatedUserId,
                IsActive = mockTest.CourseUnitMockTests.Any(),
                MockTestResult = _mapper.Map<MockTestResultModel>(mockTest.MockTestResults.FirstOrDefault(y => y.StudentId == studentId && y.CourseId == courseId))
            };
        }

        public FinalTestModel? GetFinalTest(FinalTest? finalTest, Guid? studentId, Guid courseId)
        {
            if (finalTest == null)
            {
                return null;
            }
            return new FinalTestModel
            {
                Id = finalTest.Id,
                Name = finalTest.Name,
                IsActive = finalTest.CourseUnitMockTests.Any(),
                FinalTestLevel = finalTest.FinalTestLevel,
                CreatedDate = finalTest.CreatedDate,
                CreatedFullName = finalTest.CreatedFullName,
                ExecutionTime = finalTest.ExecutionTime,
                FinalTestResult = _mapper.Map<FinalTestResultModel>(finalTest.FinalTestResults.FirstOrDefault(y => y.StudentId == studentId && y.CourseId == courseId))
            };
        }
    }
}
