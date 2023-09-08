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
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(@class));
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
                course.CourseResults.Add(new CourseResult
                {
                    StudentId = studentId ?? default,
                    CourseId = @class.CourseId,
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
            course = await _courseRepository.Queryable
                        .Include(x => x.CourseResults.Where(y => y.StudentId == studentId && y.CourseId == @class.CourseId))
                        .Include(x => x.CourseUnitMockTests)
                        .ThenInclude(x => x.Unit)
                        .ThenInclude(x => x!.UnitResults.Where(y => y.StudentId == studentId && y.CourseId == @class.CourseId))
                        .Include(x => x.CourseUnitMockTests)
                        .ThenInclude(x => x.MockTest)
                        .ThenInclude(x => x!.MockTestResults.Where(y => y.StudentId == studentId && y.CourseId == @class.CourseId))
                        .Include(x => x.CourseUnitMockTests)
                        .ThenInclude(x => x.FinalTest)
                        .ThenInclude(x => x!.FinalTestResults.Where(y => y.StudentId == studentId && y.CourseId == @class.CourseId))
                        .Include(x => x.CourseTeachers)
                        .Where(x => x.Id == @class.CourseId)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(cancellationToken);
            var courseModel = new CourseModel
            {
                Id = course!.Id,
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
                    FinalTest = x.FinalTest != null ? new FinalTestModel
                    {
                        Id = x.FinalTest.Id,
                        Name = x.FinalTest.Name,
                        IsActive = x.FinalTest.CourseUnitMockTests.Any(),
                        FinalTestLevel = x.FinalTest.FinalTestLevel,
                        CreatedDate = x.FinalTest.CreatedDate,
                        CreatedFullName = x.FinalTest.CreatedFullName,
                        ExecutionTime = x.FinalTest.ExecutionTime,
                        FinalTestResult = _mapper.Map<FinalTestResultModel>(x.FinalTest.FinalTestResults.FirstOrDefault(y => y.StudentId == studentId && y.CourseId == @class.CourseId))
                    } : null,
                    MockTest = x.MockTest != null ? new MockTestModel
                    {
                        Id = x.MockTest.Id,
                        Name = x.MockTest.Name,
                        MockTestType = x.MockTest.MockTestType,
                        CreatedDate = x.MockTest.CreatedDate,
                        CreatedFullName = x.MockTest.CreatedFullName,
                        CreatedUserId = x.MockTest.CreatedUserId,
                        IsActive = x.MockTest.CourseUnitMockTests.Any(),
                        MockTestResult = _mapper.Map<MockTestResultModel>(x.MockTest.MockTestResults.FirstOrDefault(y => y.StudentId == studentId && y.CourseId == @class.CourseId))
                    } : null,
                    Unit = x.Unit != null ? new UnitModel
                    {
                        Id = x.Unit.Id,
                        Name = x.Unit.Name,
                        Code = x.Unit.Code,
                        CourseLevel = x.Unit.CourseLevel,
                        CreatedDate = x.Unit.CreatedDate,
                        CreatedFullName = x.Unit.CreatedFullName,
                        CreatedUserId = x.Unit.CreatedUserId,
                        IsActive = x.Unit.CourseUnitMockTests.Any(),
                        UnitResult = _mapper.Map<UnitResultModel>(x.Unit.UnitResults.FirstOrDefault(y => y.StudentId == studentId && y.CourseId == @class.CourseId))
                    } : null,
                    Type = x.FinalTest != null ? nameof(x.FinalTest) : x.MockTest != null ? nameof(x.MockTest) : x.Unit != null ? nameof(x.Unit) : null
                }).ToList(),
                CourseTeachers = _mapper.Map<List<CourseTeacherModel>>(course.CourseTeachers),
                CourseResult = _mapper.Map<CourseResultModel>(course.CourseResults.FirstOrDefault(x => x.StudentId == studentId)),
            };

            var teachersResult = await _userService.GetTeacherByIdsAsync(new GetTeacherByIdsQueryModel { Ids = course?.CourseTeachers?.Select(x => x.TeacherId).ToList() });
            var teachers = teachersResult?.Content?.Result;

            if (teachersResult != null && teachersResult.IsSuccessStatusCode && teachers != null && course?.CourseTeachers != null)
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
    }
}
