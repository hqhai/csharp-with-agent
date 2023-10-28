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
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Models.CommandModels.CourseUnitMockTests;
    using Fsel.Shared.Enums;

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
        private const string UNITID_KEY = "UnitId";
        private const string MOCKTESTID_KEY = "MockTestId";
        private const string FINALTESTID_KEY = "FinalTestId";
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
            var (unitIds, unitUnFinished) = await InitListCategories(courseUnitMockTests, UNITID_KEY, request.CourseUnitMockTests);
            if (unitIds.Count == 0)
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
            var unitResults = await _unitResultRepository.Queryable.Where(x => x.CourseId == request.Id && unitUnFinished.Contains(x.UnitId)).ToListAsync();
            if (unitResults.Any() && unitResults.Any(x => x.Status != EnumResultStatus.Unfinished))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(unitResults));
                return methodResult;
            }
            #endregion validate Unit

            if (request.CourseLevel.GetEnumCourseType() == Shared.Enums.EnumCourseType.Ielts)
            {
                #region validate mockTest
                if (request.CourseUnitMockTests.Count < 10)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.Requires13CourseUnitMockTests));
                    return methodResult;
                }
                var (mocktestIds, mocktestUnFinished) = await InitListCategories(courseUnitMockTests, MOCKTESTID_KEY, request.CourseUnitMockTests);
                if (mocktestIds.Any())
                {
                    if (mocktestIds.Count > 2)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.MockTestIsUpToTwo), nameof(mocktestIds));
                        return methodResult;
                    }
                    // MockTest nếu có thì bắt buộc ở 2 vị trí 5 và 10 nếu ko phải MockTest Thì báo lỗi
                    // MockTest ở các vị trí khác thì báo lỗi
                    foreach (var i in request.CourseUnitMockTests)
                    {
                        if(i.DisplayOrder != 4 && i.DisplayOrder != 9)
                        {
                            if(i.MockTestId != null)
                            {
                                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(mocktestIds));
                                return methodResult;
                            }
                        }
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

                    var mocktestResults = await _mockTestResultRepository.Queryable.Where(x => x.CourseId == request.Id && mocktestUnFinished.Contains(x.MockTestId)).ToListAsync();
                    if (mocktestResults.Any() && mocktestResults.Any(x => x.Status != EnumResultStatus.Unfinished))
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(mocktestResults));
                        return methodResult;
                    }
                }
                #endregion validate mockTest
            }
            else
            {
                #region validate finalTest
                if (request.CourseUnitMockTests.Count < 13)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.Requires13CourseUnitMockTests));
                    return methodResult;
                }
                var (finalTestIds, finalTestUnFinished) = await InitListCategories(courseUnitMockTests, FINALTESTID_KEY, request.CourseUnitMockTests);
                if (finalTestIds.Any())
                {
                    if (finalTestIds.Count > 1)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.FinalTestIsUpToOne), nameof(finalTestIds));
                        return methodResult;
                    }
                    // - Vị trí thứ 13 bắt buộc là của Final nếu ko phải là Final thì báo lỗi
                    // - Final ở các vị trí khác thì báo lỗi
                    foreach (var i in request.CourseUnitMockTests)
                    {
                        if (i.DisplayOrder != 12)
                        {
                            if (i.FinalTestId != null)
                            {
                                methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.FinalTestIdMustBeAtTheEnd), nameof(finalTestIds));
                                return methodResult;
                            }
                        }
                    }
                    if (_finalTestRepository.IsIdsInValid(finalTestIds.Where(e => e.HasValue).Select(e => e!.Value)))
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(finalTestIds));
                        return methodResult;
                    }


                    var finalTestResults = await _finalTestResultRepository.Queryable.Where(x => x.CourseId == request.Id && finalTestUnFinished.Contains(x.FinalTestId)).ToListAsync();
                    if (finalTestResults.Any() && finalTestResults.Any(x => x.Status != EnumResultStatus.Unfinished))
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(finalTestResults));
                        return methodResult;
                    }
                }
              
                #endregion validate finalTest
            }

            return methodResult;
        }

        /// <summary>
        /// Hàm chung lấy dữ liệu của các loại category
        /// </summary>
        /// <param name="courseUnitMockTests"></param>
        /// <param name="nameProperty"></param>
        /// <param name="courseUnitMockTestsReq"></param>
        /// <returns></returns>
        public async Task<(List<Guid?> categoryIds, List<Guid?> listUnFinishedIds)> InitListCategories(List<CourseUnitMockTest> courseUnitMockTests, string nameProperty, IList<UpdateCourseUnitMockTestCommandModel>? courseUnitMockTestsReq)
        {
            var listCategoryIds = courseUnitMockTests.Where(x => x.GetPropValue(nameProperty) != null).Select(x => (Guid?)x.GetPropValue(nameProperty)).ToList();
            var categoryIds = courseUnitMockTestsReq.Where(e => e.GetPropValue(nameProperty) != null).Select(x => (Guid?)x.GetPropValue(nameProperty)).ToList();
            var listUnFinishedIds = listCategoryIds.Except(categoryIds).ToList();
            return (categoryIds, listUnFinishedIds);

        }
    }
}
