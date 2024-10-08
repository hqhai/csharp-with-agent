// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common
{
    using System;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.Units;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Microsoft.EntityFrameworkCore;

    public class UnitHelper
    {
        private readonly IUnitRepository _unitRepository;
        private readonly IMapper _mapper;
        private readonly ILessonRepository _lessonRepository;
        private readonly IMockTestRepository _mockTestRepository;

        public UnitHelper(IUnitRepository unitRepository,
            IMapper mapper,
            ILessonRepository lessonRepository,
            IMockTestRepository mockTestRepository)
        {
            _unitRepository = unitRepository;
            _mapper = mapper;
            _lessonRepository = lessonRepository;
            _mockTestRepository = mockTestRepository;
        }

        public async Task<VoidMethodResult> Validate(UpdateUnitCommandModel? request)
        {
            ArgumentNullException.ThrowIfNull(request);
            VoidMethodResult methodResult = new VoidMethodResult();

            var countLesson = request.LessonIds?.Count ?? default;
            if (request.LessonIds == null || countLesson == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.LessonIds));
                return methodResult;
            }
            switch (request.CourseLevel.GetEnumCourseType())
            {
                case EnumCourseType.Ielts:
                    if (countLesson > 4)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumUnitErrorCode.InvalidLessonQuantity), nameof(request.LessonIds), countLesson);
                        return methodResult;
                    }
                    break;

                case EnumCourseType.Academic:
                    if (countLesson > 6)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumUnitErrorCode.InvalidLessonQuantity), nameof(request.LessonIds), countLesson);
                        return methodResult;
                    }
                    break;

                case EnumCourseType.AdultFoundation:
                    if (countLesson > 5)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumUnitErrorCode.InvalidLessonQuantity), nameof(request.LessonIds), countLesson);
                        return methodResult;
                    }
                    break;
            }
            if (_lessonRepository.IsIdsInValid(request.LessonIds))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.LessonIds), request.LessonIds);
                return methodResult;
            }
            if (request.MockTestId.HasValue)
            {
                var mockTest = await _mockTestRepository.Queryable.FirstOrDefaultAsync(x => x.MockTestType == EnumMockTestType.SkillMockTest && x.Id == request.MockTestId);
                if (mockTest == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumMockTestErrorCode.MockTestExistsOtherThanTypeSkillMockTest), nameof(request.MockTestId), request.MockTestId);
                    return methodResult;
                }
            }
            if (await _unitRepository.Queryable.AnyAsync(x => x.Code == request.Code && x.CourseLevel == request.CourseLevel && (request.Id == Guid.Empty || x.Id != request.Id)))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(request.Code), request.Code);
                return methodResult;
            }

            return methodResult;
        }

        public void SetUnitData(Unit? unit, UpdateUnitCommandModel? request)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(unit);
            unit.UnitLessons = request.LessonIds?.Select((x, index) => new UnitLesson
            {
                DisplayOrder = index + 1,
                LessonId = x
            }).ToList() ?? new List<UnitLesson>();
            if (!request.MockTestId.HasValue)
            {
                unit.UnitSkillMockTests = new List<UnitSkillMockTest>();
            }
            else if (!unit.UnitSkillMockTests.Any() || unit.UnitSkillMockTests.Any(x => x.MockTestId != request.MockTestId.Value))
            {
                unit.UnitSkillMockTests = new List<UnitSkillMockTest>
                {
                    new UnitSkillMockTest
                    {
                        MockTestId = request.MockTestId.Value
                    }
                };
            }
        }
    }
}
