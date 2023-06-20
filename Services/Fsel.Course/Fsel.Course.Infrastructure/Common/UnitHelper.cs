// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common
{
    using System;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.Units;
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

        public async Task<VoidMethodResult> Validate(dynamic unit, UpdateUnitCommandModel? request)
        {
            ArgumentNullException.ThrowIfNull(request);
            VoidMethodResult methodResult = new VoidMethodResult();

            if (!unit.IsValid())
            {
                methodResult.AddErrorBadRequest(unit.ErrorMessages);
                return methodResult;
            }

            if (request.LessonIds == null || request.LessonIds.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.LessonsNull), nameof(request.LessonIds), request.LessonIds);
                return methodResult;
            }

            if (_lessonRepository.IsIdsInValid(request.LessonIds))
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.LessonsNotExist), nameof(request.LessonIds), request.LessonIds);
                return methodResult;
            }
            if (request.MockTestId != null)
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
                methodResult.AddErrorBadRequest(nameof(EnumMockTestErrorCode.CodeAndLevelAlreadyExist), nameof(request.Code), request.Code);
                return methodResult;
            }

            return methodResult;
        }
    }
}
