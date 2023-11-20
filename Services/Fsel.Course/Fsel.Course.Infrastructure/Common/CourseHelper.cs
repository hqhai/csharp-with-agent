// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.Courses;
    using Fsel.Shared.Helpers;
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

        public async Task<VoidMethodResult> Validate(dynamic course, UpdateCourseCommandModel? request)
        {
            ArgumentNullException.ThrowIfNull(request);
            VoidMethodResult methodResult = new VoidMethodResult();

            if (!course.IsValid())
            {
                methodResult.AddErrorBadRequest(course.ErrorMessages);
                return methodResult;
            }

            #region validate request

            if (request.CourseUnitMockTests == null || request.CourseUnitMockTests.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.CourseUnitMockTests));
                return methodResult;
            }

            if (request.CourseTeachers == null || request.CourseTeachers.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.CourseTeachers));
                return methodResult;
            }

            if (request.CourseUnitMockTests.Any(x => x.MockTestId.HasValue && x.UnitId.HasValue && x.FinalTestId.HasValue))
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseUnitMockTestErrorCode.MocktestIdAndUnitIdCannotCoexist), nameof(request.CourseUnitMockTests), request.CourseUnitMockTests);
                return methodResult;
            }

            if (await _courseRepository.Queryable.AnyAsync(x => x.Code == request.Code && (request.Id == Guid.Empty || x.Id != request.Id)))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(request.Code));
                return methodResult;
            }

            #endregion validate request

            #region validate Unit

            var unitIds = request.CourseUnitMockTests.Where(e => e.UnitId != null).Select(x => x.UnitId).ToList();
            var units = await _unitRepository.Queryable.Where(x => unitIds.Contains(x.Id)).ToListAsync();
            if (units == null || units.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }
            if (unitIds.Count != unitIds.Distinct().Count())
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.DuplicateUnitId));
                return methodResult;
            }

            if (unitIds.Distinct().Count() > 8)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.UnitTestIsUpToEight));
                return methodResult;
            }

            if (units.Count != unitIds.Count)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(units));
                return methodResult;
            }

            var isCheck = units.All(x => unitIds.Contains(x.Id) && x.CourseLevel == request.CourseLevel);
            if (!isCheck)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.AnotherLevelUnitExists));
                return methodResult;
            }

            #endregion validate Unit

            if (request.CourseLevel.GetEnumCourseType() == Shared.Enums.EnumCourseType.Ielts)
            {
                #region validate mockTest

                var mocktestIds = request.CourseUnitMockTests.Where(e => e.MockTestId != null).Select(x => x.MockTestId).Distinct().ToList();
                if (mocktestIds.Count > 2)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.MockTestIsUpToTwo), nameof(mocktestIds));
                    return methodResult;
                }

                if (mocktestIds.Count == 0)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(mocktestIds));
                    return methodResult;
                }

                if (_mockTestRepository.IsIdsInValid(mocktestIds.Where(e => e.HasValue).Select(e => e!.Value)))
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(mocktestIds));
                    return methodResult;
                }

                var mocktests = await _mockTestRepository.Queryable.Include(x => x.CourseUnitMockTests).Where(x => mocktestIds != null && mocktestIds.Contains(x.Id)).ToListAsync();
                var checkMockTest = mocktests.All(x => x.MockTestType == EnumMockTestType.FullMockTest);
                if (!checkMockTest)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumMockTestErrorCode.MockTestExistsOtherThanTypeFullMockTest), nameof(mocktests), mocktests);
                    return methodResult;
                }

                #endregion validate mockTest
            }
            else
            {
                #region validate finalTest

                var finalTestIds = request.CourseUnitMockTests.Where(e => e.FinalTestId != null).Select(x => x.FinalTestId).Distinct().ToList();
                if (_finalTestRepository.IsIdsInValid(finalTestIds.Where(e => e.HasValue).Select(e => e!.Value)))
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(finalTestIds));
                    return methodResult;
                }

                if (finalTestIds.Count == 0)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(finalTestIds));
                    return methodResult;
                }

                if (finalTestIds.Count > 1)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.FinalTestIsUpToOne), nameof(finalTestIds));
                    return methodResult;
                }

                #endregion validate finalTest
            }

            return methodResult;
        }
    }
}
