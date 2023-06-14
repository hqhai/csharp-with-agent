// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.Courses;
    using Microsoft.EntityFrameworkCore;

    public class CourseHelper
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly IMapper _mapper;
        private readonly IFinalTestRepository _finalTestRepository;
        private readonly IMockTestRepository _mockTestRepository;

        public CourseHelper(ICourseRepository courseRepository
            , IUnitRepository unitRepository
            , IMapper mapper
            , IFinalTestRepository finalTestRepository
            , IMockTestRepository mockTestRepository)
        {
            _courseRepository = courseRepository;
            _unitRepository = unitRepository;
            _mapper = mapper;
            _finalTestRepository = finalTestRepository;
            _mockTestRepository = mockTestRepository;
        }

        public async Task<VoidMethodResult> CourseValue(dynamic course, UpdateCourseCommandModel? request)
        {
            ArgumentNullException.ThrowIfNull(request);
            VoidMethodResult methodResult = new VoidMethodResult();

            if (!course.IsValid())
            {
                methodResult.AddErrorBadRequest(course.ErrorMessages);
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

            if (request.CourseUnitMockTests.Any(x => x.MockTestId.HasValue && x.UnitId.HasValue && x.FinalTestId.HasValue))
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseUnitMockTestErrorCode.MocktestIdAndUnitIdCannotCoexist), nameof(request.CourseUnitMockTests), request.CourseUnitMockTests);
                return methodResult;
            }

            if (await _courseRepository.Queryable.AnyAsync(x => x.Code == request.Code && x.CourseLevel == request.CourseLevel && (request.Id == Guid.Empty || x.Id != request.Id)))
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.CourseCodeIsExist), nameof(request.Code), request.Code);
                return methodResult;
            }

            var units = request.CourseUnitMockTests.Where(e => e.UnitId != null).Select(x => x.UnitId).Distinct().ToList();
            if (_unitRepository.IsIdsInValid(units.Where(e => e.HasValue).Select(e => e!.Value)))
            {
                methodResult.AddErrorBadRequest(nameof(EnumUnitErrorCode.UnitsNotExist), nameof(units), units);
                return methodResult;
            }

            var mocktestIds = request.CourseUnitMockTests.Where(e => e.MockTestId != null).Select(x => x.MockTestId).Distinct().ToList();
            if (_mockTestRepository.IsIdsInValid(mocktestIds.Where(e => e.HasValue).Select(e => e!.Value)))
            {
                methodResult.AddErrorBadRequest(nameof(EnumMockTestErrorCode.MockTestsNotExist), nameof(mocktestIds), mocktestIds);
                return methodResult;
            }

            var finalTestIds = request.CourseUnitMockTests.Where(e => e.FinalTestId != null).Select(x => x.FinalTestId).Distinct().ToList();
            if (_finalTestRepository.IsIdsInValid(finalTestIds.Where(e => e.HasValue).Select(e => e!.Value)))
            {
                methodResult.AddErrorBadRequest(nameof(EnumFinalTestErrorCode.FinalTestsNotExist), nameof(finalTestIds), finalTestIds);
                return methodResult;
            }

            var mocktests = await _mockTestRepository.Queryable.Include(x => x.CourseUnitMockTests).Where(x => mocktestIds != null && mocktestIds.Contains(x.Id)).ToListAsync();
            var checkMockTest = mocktests.All(x => x.MockTestType == EnumMockTestType.FullMockTest);
            if (!checkMockTest)
            {
                methodResult.AddErrorBadRequest(nameof(EnumMockTestErrorCode.MockTestExistsOtherThanTypeFullMockTest), nameof(mocktests), mocktests);
                return methodResult;
            }

            return methodResult;
        }
    }
}
