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
    using Fsel.Course.Domain.Entities;
    public class CourseHelper
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly IMapper _mapper;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly IFinalTestResultRepository _finalTestResultRepository;
        private readonly IFinalTestRepository _finalTestRepository;
        private readonly IMockTestRepository _mockTestRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;

        public CourseHelper(ICourseRepository courseRepository
            , IUnitRepository unitRepository
            , IMapper mapper
            ,IUnitResultRepository unitResultRepository
            , IFinalTestRepository finalTestRepository
            ,IFinalTestResultRepository finalTestResultRepository
            , IMockTestResultRepository mockTestResultRepository
            , IMockTestRepository mockTestRepository)
        {
            _courseRepository = courseRepository;
            _unitRepository = unitRepository;
            _mapper = mapper;
            _finalTestResultRepository = finalTestResultRepository;
            _unitResultRepository = unitResultRepository;
            _finalTestRepository = finalTestRepository;
            _mockTestRepository = mockTestRepository;
            _mockTestResultRepository = mockTestResultRepository;
        }
        public async Task<VoidMethodResult> Validate(Course course, UpdateCourseCommandModel? request)
        {
            ArgumentNullException.ThrowIfNull(request);
            VoidMethodResult methodResult = new VoidMethodResult();

            var courseUnitMockTests = course.CourseUnitMockTests.ToList();
            if(courseUnitMockTests == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
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

            if (await _courseRepository.Queryable.AnyAsync(x => x.Code == request.Code && x.CourseLevel == request.CourseLevel && (request.Id == Guid.Empty || x.Id != request.Id)))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Code));
                return methodResult;
            }

            #endregion validate request

            #region validate Unit
            var listUnit = courseUnitMockTests.Select(x => x.UnitId).ToList();
            var unitIds = request.CourseUnitMockTests.Where(e => e.UnitId != null).Select(x => x.UnitId).ToList();
            var unitUnfinished = listUnit.Except(unitIds).ToList();

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
            var unitResults = await _unitResultRepository.Queryable.Where(x => x.CourseId == request.Id && unitUnfinished.Contains(x.UnitId)).ToListAsync();
            if(unitResults.Any() && unitResults.Any(x=>x.Status != EnumResultStatus.Unfinished))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist),nameof(unitResults));
                return methodResult;
            }
            #endregion validate Unit

            if (request.CourseLevel.GetEnumCourseType() == Shared.Enums.EnumCourseType.Ielts)
            {
                #region validate mockTest
                var listMocktest = courseUnitMockTests.Select(x => x.MockTestId).ToList();
                var mocktestIds = request.CourseUnitMockTests.Where(e => e.MockTestId != null).Select(x => x.MockTestId).Distinct().ToList();
                var mocktestUnfinished = listMocktest.Except(mocktestIds).ToList();
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

                var mocktestResults = await _mockTestResultRepository.Queryable.Where(x => x.CourseId == request.Id && mocktestUnfinished.Contains(x.MockTestId)).ToListAsync();
                if (mocktestResults.Any() && mocktestResults.Any(x => x.Status != EnumResultStatus.Unfinished))
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(mocktestResults));
                    return methodResult;
                }
                #endregion validate mockTest
            }
            else
            {
                #region validate finalTest
                var listFinalTest = courseUnitMockTests.Select(x => x.FinalTestId).ToList();
                var finalTestIds = request.CourseUnitMockTests.Where(e => e.FinalTestId != null).Select(x => x.FinalTestId).Distinct().ToList();
                var finalTestUnfinished = listFinalTest.Except(finalTestIds).ToList();
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
                var finalTestResults = await _finalTestResultRepository.Queryable.Where(x => x.CourseId == request.Id && finalTestUnfinished.Contains(x.FinalTestId)).ToListAsync();
                if (finalTestResults.Any() && finalTestResults.Any(x => x.Status != EnumResultStatus.Unfinished))
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(finalTestResults));
                    return methodResult;
                }
                #endregion validate finalTest
            }

            return methodResult;
        }
    }
}
