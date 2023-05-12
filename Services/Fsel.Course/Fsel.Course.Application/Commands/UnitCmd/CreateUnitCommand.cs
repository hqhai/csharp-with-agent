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

        public CreateUnitCommandHandler(IUnitRepository unitRepository
            , ILessonRepository lessonRepository
            , IMockTestRepository mockTestRepository
            , IMapper mapper)
        {
            _lessonRepository = lessonRepository;
            _unitRepository = unitRepository;
            _mockTestRepository = mockTestRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<UnitModel>> Handle(CreateUnitCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<UnitModel> methodResult = new MethodResult<UnitModel>();

            #region Validation

            Unit unit = _mapper.Map<Unit>(request);

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

            var checkMockTest = _mockTestRepository.Queryable.Any(x => x.MockTestType == EnumMockTestType.SkillMockTest && x.Id == request.MockTestId);
            if (!checkMockTest)
            {
                methodResult.AddErrorBadRequest(nameof(EnumMockTestErrorCode.MockTestExistsOtherThanTypeSkillMockTest), nameof(request.MockTestId), request.MockTestId);
                return methodResult;
            }

            #endregion Validation

            await _unitRepository.ExecuteTransactionAsync(async () =>
            {
                unit.UnitLessons = request.LessonIds.Select((x, index) => new UnitLesson
                {
                    DisplayOrder = index,
                    LessonId = x
                }).ToList();

                unit.UnitSkillMockTests = new List<UnitSkillMockTest>
                {
                    new UnitSkillMockTest
                    {
                        MockTestId = request.MockTestId,
                    }
                };

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
