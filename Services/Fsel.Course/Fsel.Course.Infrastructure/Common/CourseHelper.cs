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
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.Courses;
    using Fsel.Course.Domain.Models.CommandModels.CourseUnitMockTests;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Microsoft.EntityFrameworkCore;

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
            , IUnitResultRepository unitResultRepository
            , IFinalTestRepository finalTestRepository
            , IFinalTestResultRepository finalTestResultRepository
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

            if (request.CourseLevel.GetEnumCourseType() == EnumCourseType.Ielts)
            {
                if (unitIds.Count > 8)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.InvalidUnitQuantity));
                    return methodResult;
                }

                #region validate mockTest

                var mocktestIds = request.CourseUnitMockTests.Where(e => e.MockTestId != null).Select(x => x.MockTestId).Distinct().ToList();
                if (mocktestIds.Count > 2)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.MockTestCannotBeDreaterThan2), nameof(mocktestIds));
                    return methodResult;
                }

                if (mocktestIds == null || mocktestIds.Count == 0)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(mocktestIds));
                    return methodResult;
                }

                if (_mockTestRepository.IsIdsInValid(mocktestIds.Where(e => e.HasValue).Select(e => e!.Value)))
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(mocktestIds));
                    return methodResult;
                }

                var mocktests = await _mockTestRepository.Queryable.Include(x => x.CourseUnitMockTests).Where(x => mocktestIds.Contains(x.Id)).ToListAsync();
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
                if (unitIds.Count > 12)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.InvalidUnitQuantity));
                    return methodResult;
                }

                #region validate finalTest

                var finalTestIds = request.CourseUnitMockTests.Where(e => e.FinalTestId != null).Select(x => x.FinalTestId).Distinct().ToList();
                if (_finalTestRepository.IsIdsInValid(finalTestIds.Where(e => e.HasValue).Select(e => e!.Value)))
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(finalTestIds));
                    return methodResult;
                }

                if (finalTestIds == null || finalTestIds.Count == 0)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(finalTestIds));
                    return methodResult;
                }

                if (finalTestIds.Count > 1)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.FinalTestCannotBeDreaterThan1), nameof(finalTestIds));
                    return methodResult;
                }

                #endregion validate finalTest
            }

            return methodResult;
        }

        public async Task<VoidMethodResult> ValidateV1i1(Course course, UpdateCourseCommandModel? request)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(course);
            VoidMethodResult methodResult = new VoidMethodResult();

            var courseUnitMockTests = course.CourseUnitMockTests.ToList();

            #region validate request

            if (request.CourseUnitMockTests == null || request.CourseUnitMockTests.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.CourseUnitMockTests), request.CourseUnitMockTests);
                return methodResult;
            }

            if (request.CourseTeachers == null || request.CourseTeachers.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.CourseTeachers), request.CourseTeachers);
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

            var (unitIds, unitUnFinished) = InitListCategories(courseUnitMockTests, nameof(CourseUnitMockTest.UnitId), request.CourseUnitMockTests);
            if (unitIds == null || unitIds.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(unitIds));
                return methodResult;
            }
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

            var unitResults = await _unitResultRepository.Queryable.Where(x => x.CourseId == request.Id && unitUnFinished.Contains(x.UnitId)).ToListAsync();
            if (unitResults.Any() && unitResults.Any(x => x.Status != EnumResultStatus.Unfinished))
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.UnitHasBeenUsed), nameof(unitResults));
                return methodResult;
            }

            #endregion validate Unit

            switch (request.CourseLevel.GetEnumCourseType())
            {
                case EnumCourseType.Ielts:
                    if (unitIds.Count > 8)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.InvalidUnitQuantity), nameof(unitIds), unitIds.Count);
                        return methodResult;
                    }
                    if (request.CourseUnitMockTests.Any(x => x.FinalTestId.HasValue))
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(MockTest));
                        return methodResult;
                    }

                    var methodIelts = await ValidateMockTestV1i1(courseUnitMockTests, request);
                    if (!methodIelts.IsOK)
                    {
                        methodResult.AddErrorBadRequest(methodIelts.ErrorMessages);
                        return methodResult;
                    }
                    break;

                case EnumCourseType.Academic:
                case EnumCourseType.EnglishFoundation:
                    if (request.CourseLevel == EnumCourseLevel.EFA1 && unitIds.Count > 10)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.InvalidUnitQuantity), nameof(unitIds), unitIds.Count);
                        return methodResult;
                    }
                    else if (unitIds.Count > 12)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.InvalidUnitQuantity), nameof(unitIds), unitIds.Count);
                        return methodResult;
                    }
                    if (request.CourseUnitMockTests.Any(x => x.MockTestId.HasValue))
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(FinalTest));
                        return methodResult;
                    }
                    var methodAdultFoundation = await ValidateFinalTestV1i1(courseUnitMockTests, request);
                    if (!methodAdultFoundation.IsOK)
                    {
                        methodResult.AddErrorBadRequest(methodAdultFoundation.ErrorMessages);
                        return methodResult;
                    }
                    break;
            }

            return methodResult;
        }

        public async Task<VoidMethodResult> ValidateFinalTestV1i1(IList<CourseUnitMockTest>? courseUnitMockTests, UpdateCourseCommandModel? request)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.CourseUnitMockTests);
            ArgumentNullException.ThrowIfNull(courseUnitMockTests);

            VoidMethodResult methodResult = new VoidMethodResult();
            if (request.CourseUnitMockTests.Count < 13)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.Requires13CourseUnitMockTests));
                return methodResult;
            }
            var (finalTestIds, finalTestUnFinished) = InitListCategories(courseUnitMockTests, nameof(CourseUnitMockTest.FinalTestId), request.CourseUnitMockTests);
            if (finalTestIds != null && finalTestIds.Any() && finalTestUnFinished != null)
            {
                if (finalTestIds.Count > 1)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.FinalTestCannotBeDreaterThan1), nameof(finalTestIds));
                    return methodResult;
                }
                // - Vị trí thứ 13 bắt buộc là của Final nếu ko phải là Final thì báo lỗi
                // - Final ở các vị trí khác thì báo lỗi
                if (request.CourseUnitMockTests.Any(i => i.DisplayOrder != 13 && i.FinalTestId.HasValue))
                {
                    methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.FinalTestIdMustBeAtTheEnd), nameof(finalTestIds));
                    return methodResult;
                }
                if (_finalTestRepository.IsIdsInValid(finalTestIds.Select(e => e)))
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(finalTestIds));
                    return methodResult;
                }

                var finalTestResults = await _finalTestResultRepository.Queryable.Where(x => x.CourseId == request.Id && finalTestUnFinished.Contains(x.FinalTestId)).ToListAsync();
                if (finalTestResults.Any() && finalTestResults.Any(x => x.Status != EnumResultStatus.Unfinished))
                {
                    methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.FinalTestHasBeenUsed), nameof(finalTestResults));
                    return methodResult;
                }
            }
            return methodResult;
        }

        public async Task<VoidMethodResult> ValidateMockTestV1i1(IList<CourseUnitMockTest>? courseUnitMockTests, UpdateCourseCommandModel? request)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.CourseUnitMockTests);
            ArgumentNullException.ThrowIfNull(courseUnitMockTests);
            VoidMethodResult methodResult = new VoidMethodResult();

            var (mocktestIds, mocktestUnFinished) = InitListCategories(courseUnitMockTests, nameof(CourseUnitMockTest.MockTestId), request.CourseUnitMockTests);
            if (mocktestIds != null && mocktestIds.Any() && mocktestUnFinished != null)
            {
                if (mocktestIds.Count > 2)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.MockTestCannotBeDreaterThan2), nameof(mocktestIds));
                    return methodResult;
                }
                // MockTest nếu có thì bắt buộc ở 2 vị trí 5 và 10 nếu ko phải MockTest Thì báo lỗi
                // MockTest ở các vị trí khác thì báo lỗi
                if (request.CourseUnitMockTests.Any(i => i.DisplayOrder != 4 && i.DisplayOrder != 9 && i.MockTestId.HasValue))
                {
                    methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.MockTestMustBeInPositions5And10), nameof(mocktestIds));
                    return methodResult;
                }

                if (_mockTestRepository.IsIdsInValid(mocktestIds.Select(e => e)))
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(mocktestIds));
                    return methodResult;
                }

                var mocktests = await _mockTestRepository.Queryable.Include(x => x.CourseUnitMockTests).Where(x => mocktestIds.Contains(x.Id)).ToListAsync();
                var checkMockTest = mocktests.All(x => x.MockTestType == EnumMockTestType.FullMockTest);
                if (!checkMockTest)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumMockTestErrorCode.MockTestExistsOtherThanTypeFullMockTest), nameof(mocktests), mocktests);
                    return methodResult;
                }

                var mocktestResults = await _mockTestResultRepository.Queryable.Where(x => x.CourseId == request.Id && mocktestUnFinished.Contains(x.MockTestId)).ToListAsync();
                if (mocktestResults.Any() && mocktestResults.Any(x => x.Status != EnumResultStatus.Unfinished))
                {
                    methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.MockTestResultsExist), nameof(mocktestResults));
                    return methodResult;
                }
            }
            return methodResult;
        }

        public (List<Guid>? categoryIds, List<Guid>? listUnFinishedIds) InitListCategories(IList<CourseUnitMockTest>? courseUnitMockTests, string nameProperty, IList<UpdateCourseUnitMockTestCommandModel>? courseUnitMockTestsReq)
        {
            var listCategoryIds = courseUnitMockTests?.Where(x => x.GetPropValue<Guid?>(nameProperty) != null).Select(x => x.GetPropValue<Guid>(nameProperty)).ToList();
            var categoryIds = courseUnitMockTestsReq?.Where(e => e.GetPropValue<Guid?>(nameProperty) != null).Select(x => x.GetPropValue<Guid>(nameProperty)).ToList();
            return (categoryIds, categoryIds != null ? listCategoryIds?.Except(categoryIds).ToList() : default);
        }
    }
}
