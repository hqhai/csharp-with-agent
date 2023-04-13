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

namespace Fsel.Course.Application.Commands.UnitCmd
{
    public class UpdateUnitCommand : UpdateUnitCommandModel, IRequest<MethodResult<UnitModel>>
    {
    }

    public class UpdateUnitCommandHandler : IRequestHandler<UpdateUnitCommand, MethodResult<UnitModel>>
    {
        private readonly IUnitRepository _unitRepository;
        private readonly IMapper _mapper;
        private readonly ILessonRepository _lessonRepository;
        private readonly IMockTestRepository _mockTestRepository;

        public UpdateUnitCommandHandler(IUnitRepository unitTestRepository, ILessonRepository lessonRepository, IMockTestRepository mockTestRepository,
            IMapper mapper)
        {
            _lessonRepository = lessonRepository;
            _unitRepository = unitTestRepository;
            _mapper = mapper;
            _mockTestRepository = mockTestRepository;
        }

        public async Task<MethodResult<UnitModel>> Handle(UpdateUnitCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<UnitModel> methodResult = new MethodResult<UnitModel>();

            #region Validation

            var unit = await _unitRepository.Queryable
                                    .Include(e => e.UnitLessons.Where(n => !n.IsDeleted))
                                    .Include(e => e.UnitSkillMockTests.Where(n => !n.IsDeleted))
                                    .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken: cancellationToken);
            if (unit == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumUnitErrorCode.UnitIdNotExist), nameof(request.Id), request?.Id);
                return methodResult;
            }

            var isUnitUsed = await _unitRepository.IsUnitUsed(request.Id);
            if (isUnitUsed)
            {
                methodResult.AddErrorBadRequest(nameof(EnumUnitErrorCode.UnitUsed), nameof(request.Id), request.Id);
                return methodResult;
            }

            if (request.LessonIds == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.LessonIdsNull), nameof(request.LessonIds), request.LessonIds);
                return methodResult;
            }

            if (_lessonRepository.IsIdsInValid(request.LessonIds))
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.LessonIdsNotExist), nameof(request.LessonIds), request.LessonIds);
                return methodResult;
            }

            var checkMockTest = _mockTestRepository.Queryable.Any(x => x.MockTestType == EnumMockTestType.UnitMockTest && x.Id == request.MockTestId);
            if (!checkMockTest)
            {
                methodResult.AddErrorBadRequest(nameof(EnumMockTestErrorCode.MockTestExistsOtherThanTypeUnitMockTest), nameof(request.MockTestId), request.MockTestId);
                return methodResult;
            }

            _mapper.Map(request, unit);

            if (!unit.IsValid())
            {
                methodResult.AddErrorBadRequest(unit.ErrorMessages);
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

                unit = _unitRepository.Update(unit);
                await _unitRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<UnitModel>(unit);
                return methodResult;
            });

            return methodResult;
        }
    }
}
