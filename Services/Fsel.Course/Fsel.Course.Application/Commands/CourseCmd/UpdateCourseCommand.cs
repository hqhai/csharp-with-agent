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
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Application.Commands.CourseCmd
{
    public class UpdateCourseCommand : UpdateCourseCommandModel, IRequest<MethodResult<CourseModel>>
    {
    }

    public class UpdateCourseTestCommandHandler : IRequestHandler<UpdateCourseCommand, MethodResult<CourseModel>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;
        private readonly IUnitRepository _unitRepository;
        private readonly IUserService _userService;
        private readonly IMockTestRepository _mockTestRepository;

        public UpdateCourseTestCommandHandler(ICourseRepository courseRepository
            , IMapper mapper
            , IUnitRepository unitRepository
            , IUserService userService
            , IMockTestRepository mockTestRepository)
        {
            _courseRepository = courseRepository;
            _mapper = mapper;
            _unitRepository = unitRepository;
            _userService = userService;
            _mockTestRepository = mockTestRepository;
        }

        public async Task<MethodResult<CourseModel>> Handle(UpdateCourseCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<CourseModel> methodResult = new MethodResult<CourseModel>();

            #region Validation

            var course = _courseRepository.Queryable.Where(e => e.Id == request.Id).Include(e => e.CourseUnitMockTests).Include(e => e.CourseTeachers).FirstOrDefault();
            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.CourseIdNotExist));
                return methodResult;
            }
            _mapper.Map(request, course);

            if (!course.IsValid())
            {
                methodResult.AddErrorBadRequest(course.ErrorMessages);
                return methodResult;
            }

            if (course.Status != EnumCourseStatus.New)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.CourseNotInNewState), nameof(course.Status), course.Status);
                return methodResult;
            }

            if (request.CourseUnitMockTests == null || request.CourseUnitMockTests.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseUnitMockTestErrorCode.CourseUnitMockTestsNull), nameof(request.CourseUnitMockTests), request.CourseUnitMockTests);
                return methodResult;
            }

            if (request.CourseTeachers == null || request.CourseTeachers.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseTeacherErrorCode.CourseTeachersNull), nameof(request.CourseTeachers), request.CourseTeachers);
                return methodResult;
            }

            var isExistCode = await _courseRepository.Queryable.AnyAsync(x => x.Code == request.Code && x.Id != request.Id, cancellationToken);
            if (isExistCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.CourseCodeIsExist), nameof(request.Code), request.Code);
                return methodResult;
            }

            if (request.CourseUnitMockTests.Any(x => x.MockTestId.HasValue && x.UnitId.HasValue))
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.MocktestIdAndUnitIdAreMutuallyExclusive), nameof(request.CourseUnitMockTests), request.CourseUnitMockTests);
                return methodResult;
            }

            var units = request.CourseUnitMockTests.Where(e => e.UnitId != null).Select(x => x.UnitId).ToList();
            if (_unitRepository.IsIdsInValid(units.Where(e => e.HasValue).Select(e => e!.Value)))
            {
                methodResult.AddErrorBadRequest(nameof(EnumUnitErrorCode.UnitIdsNotExist), nameof(units), units);
                return methodResult;
            }

            var mocktestIds = request.CourseUnitMockTests.Where(e => e.MockTestId != null).Select(x => x.MockTestId);
            if (_mockTestRepository.IsIdsInValid(mocktestIds.Where(e => e.HasValue).Select(e => e!.Value)))
            {
                methodResult.AddErrorBadRequest(nameof(EnumMockTestErrorCode.MockTestIdsNotExist), nameof(mocktestIds), mocktestIds);
                return methodResult;
            }

            var mocktests = await _mockTestRepository.GetByIdsAsync(mocktestIds.Where(e => e.HasValue).Select(e => e!.Value));

            var checkMockTest = mocktests.All(x => x.MockTestType == EnumMockTestType.CourseMockTest);
            if (!checkMockTest)
            {
                methodResult.AddErrorBadRequest(nameof(EnumMockTestErrorCode.MockTestExistsOtherThanTypeCourseMocktest), nameof(mocktests), mocktests);
                return methodResult;
            }

            var teachers = await _userService.GetTeacherByIdsAsync(new GetTeacherByIdsQueryModel { Ids = course.CourseTeachers.Select(x => x.TeacherId).ToList() });
            if (!teachers.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.TeacherIdsNotExist), nameof(teachers), course.CourseTeachers.Select(x => x.TeacherId).ToList());
                return methodResult;
            }

            #endregion Validation

            await _courseRepository.ExecuteTransactionAsync(async () =>
            {
                course = _courseRepository.Update(course);

                await _courseRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<CourseModel>(course);
                return methodResult;
            });

            return methodResult;
        }
    }
}
