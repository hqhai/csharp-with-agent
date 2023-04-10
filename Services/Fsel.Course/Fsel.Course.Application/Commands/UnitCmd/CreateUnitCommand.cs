// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.Enums.ErrorCodes;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.CommandModels.Units;
using Fsel.Course.Domain.Models.EntityModels;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Unit = Fsel.Course.Domain.Entities.Unit;

namespace Fsel.Course.Application.Commands.UnitCmd
{
    public class CreateUnitCommand : CreateUnitCommandModel, IRequest<MethodResult<UnitModel>>
    {
    }

    public class CreateUnitCommandHandler : IRequestHandler<CreateUnitCommand, MethodResult<UnitModel>>
    {
        private readonly IUnitRepository _unitRepository;
        private readonly IMapper _mapper;
        private readonly ILessonRepository _lessonRepository;
        private readonly IMockTestRepository _mockTestRepository;
        private readonly Guid _courseId = new Guid("8B11384D-F28E-48EA-96F2-0C7B703ACC5F");
        private readonly ICourseUnitMockTestRepository _courseUnitMockTestRepository;

        public CreateUnitCommandHandler(IUnitRepository unitRepository
            , ILessonRepository lessonRepository
            , IMockTestRepository mockTestRepository
            , IMapper mapper
            , ICourseUnitMockTestRepository courseUnitMockTestRepository)
        {
            _lessonRepository = lessonRepository;
            _unitRepository = unitRepository;
            _mockTestRepository = mockTestRepository;
            _mapper = mapper;
            _courseUnitMockTestRepository = courseUnitMockTestRepository;
        }

        public async Task<MethodResult<UnitModel>> Handle(CreateUnitCommand request, CancellationToken cancellationToken)
        {
            MethodResult<UnitModel> methodResult = new MethodResult<UnitModel>();

            #region Validation

            Unit unit = _mapper.Map<Unit>(request);

            if (!unit.IsValid())
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddResultFromErrorList(unit.ErrorMessages);
                return methodResult;
            }

            if (request?.LessonIds == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumUnitErrorCode.UnitIdNotCorrect), nameof(request.LessonIds), request?.LessonIds);
                return methodResult;
            }

            if (_lessonRepository.IsIdsInValid(request.LessonIds))
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.LessonNotCorrect), nameof(request.LessonIds), request.LessonIds);
                return methodResult;
            }

            var checkMockTest = _mockTestRepository.Queryable.Any(x => x.MockTestType == EnumMockTestType.UnitMockTest && x.Id == request.MockTestId);
            if (!checkMockTest)
            {
                methodResult.AddErrorBadRequest(nameof(EnumMockTestErrorCode.MockTestInValid));
                return methodResult;
            }

            #endregion Validation

            var courseUnitMockTest = await _courseUnitMockTestRepository.Queryable.Where(x => x.CourseId == _courseId).OrderByDescending(x => x.DisplayOrder).FirstOrDefaultAsync(cancellationToken);

            await _unitRepository.ExecuteTransactionAsync(async () =>
            {
                unit.UnitLessons = request.LessonIds.Select(x => new UnitLesson
                {
                    LessonId = x
                }).ToList();

                unit.UnitSkillMockTests = new List<UnitSkillMockTest>
                 {
                     new UnitSkillMockTest
                     {
                         MockTestId = request.MockTestId,
                     }
                 };

                unit.CourseUnitMockTests = new List<CourseUnitMockTest>() { new CourseUnitMockTest { CourseId = _courseId, DisplayOrder = (courseUnitMockTest?.DisplayOrder ?? default) + 1 } };

                unit = _unitRepository.Add(unit);
                await _unitRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<UnitModel>(unit);
                return methodResult;
            });

            return methodResult;
        }
    }
}
