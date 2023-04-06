// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Course.Application.Services.UserServices;
using Fsel.Course.Application.Services.UserServices.Models;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.Enums.ErrorCodes;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.CommandModels.Courses;
using Fsel.Course.Domain.Models.EntityModels;
using MediatR;
using Microsoft.AspNetCore.Http;
using EntityCourse = Fsel.Course.Domain.Entities.Course;

namespace Fsel.Course.Application.Commands.CourseCmd
{
    public class CreateCourseCommand : CreateCourseCommandModel, IRequest<MethodResult<CourseModel>>
    {
    }

    public class CreateCourseCommandHandler : IRequestHandler<CreateCourseCommand, MethodResult<CourseModel>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly IMapper _mapper;
        private readonly IMockTestRepository _mockTestRepository;
        private readonly IUserService _userService;

        public CreateCourseCommandHandler(ICourseRepository courseRepository
            , IUnitRepository unitRepository
            , IMapper mapper
            , IMockTestRepository mockTestRepository
            , IUserService userService
            )
        {
            _unitRepository = unitRepository;
            _courseRepository = courseRepository;
            _mapper = mapper;
            _mockTestRepository = mockTestRepository;
            _userService = userService;
        }

        public async Task<MethodResult<CourseModel>> Handle(CreateCourseCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<CourseModel> methodResult = new MethodResult<CourseModel>();

            #region Validation

            EntityCourse course = _mapper.Map<EntityCourse>(request);

            if (!course.IsValid())
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddResultFromErrorList(course.ErrorMessages);
                return methodResult;
            }

            if (request.CourseUnitMockTests == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseUnitMockTestErrorCode.CourseUnitMockTestIsNull), nameof(request.CourseUnitMockTests));
                return methodResult;
            }

            if (request.CourseTeachers == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseTeacherErrorCode.CourseTeacherIsNull), nameof(request.CourseTeachers));
                return methodResult;
            }

            if (request.CourseUnitMockTests.Any(x => x.MockTestId.HasValue && x.UnitId.HasValue))
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseUnitMockTestErrorCode.MocktestIdAndUnitIdAreMutuallyExclusive));
                return methodResult;
            }

            var units = request.CourseUnitMockTests.Where(e => e.UnitId != null).Select(x => x.UnitId).ToList();
            if (_unitRepository.IsIdsInValid(units.Where(e => e.HasValue).Select(e => e!.Value)))
            {
                methodResult.AddErrorBadRequest(nameof(EnumUnitErrorCode.UnitIdNotCorrect), nameof(request.CourseUnitMockTests));
                return methodResult;
            }

            var mocktestIds = request.CourseUnitMockTests.Where(e => e.MockTestId != null).Select(x => x.MockTestId);
            if (_mockTestRepository.IsIdsInValid(mocktestIds.Where(e => e.HasValue).Select(e => e!.Value)))
            {
                methodResult.AddErrorBadRequest(nameof(EnumMockTestErrorCode.TestNotCorrect), nameof(request.CourseUnitMockTests));
                return methodResult;
            }

            var mocktests = await _mockTestRepository.GetByIdsAsync(mocktestIds.Where(e => e.HasValue).Select(e => e!.Value));

            var checkMockTest = mocktests.All(x => x.MockTestType == EnumMockTestType.CourseMockTest);
            if (!checkMockTest)
            {
                methodResult.AddErrorBadRequest(nameof(EnumMockTestErrorCode.TestInValid), nameof(request.CourseUnitMockTests), mocktests.Select(x => x.Id));
                return methodResult;
            }

            var teachersResult = await _userService.GetTeacherByIdsAsync(new GetTeacherByIdsQueryModel { Ids = course.CourseTeachers.Select(x => x.TeacherId).ToList() });
            if (!teachersResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.ListTeacherCourseNotExist), nameof(request.CourseTeachers));
                return methodResult;
            }
            var teacherNames = teachersResult?.Content?.Result?.Select(x => x.Human?.FullName).ToList();
            var nameTeacher = string.Join(", ", teacherNames ?? new List<string?>());

            course.Name = $"{course.CourseLevel}-" + nameTeacher;

            #endregion Validation

            await _courseRepository.ExecuteTransactionAsync(async () =>
            {
                course = _courseRepository.Add(course);

                await _courseRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<CourseModel>(course);
                return methodResult;
            });

            return methodResult;
        }
    }
}
