// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums.ErrorCodes;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.CommandModels.Units;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Course.Infrastructure.Common;
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
        private readonly UnitHelper _unitHelper;
        private readonly IMapper _mapper;

        public UpdateUnitCommandHandler(IUnitRepository unitTestRepository,
            UnitHelper unitHelper,
            IMapper mapper)
        {
            _unitRepository = unitTestRepository;
            _unitHelper = unitHelper;
            _mapper = mapper;
        }

        public async Task<MethodResult<UnitModel>> Handle(UpdateUnitCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<UnitModel> methodResult = new MethodResult<UnitModel>();

            #region Validation

            if (request.LessonIds == null || !request.LessonIds.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.LessonIds), request.LessonIds);
                return methodResult;
            }

            var unit = await _unitRepository.Queryable
                                    .Include(e => e.UnitLessons)
                                    .Include(e => e.UnitSkillMockTests)
                                    .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken: cancellationToken);
            if (unit == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(unit));
                return methodResult;
            }

            var isUnitUsed = await _unitRepository.IsUnitUsed(request.Id);
            if (isUnitUsed)
            {
                methodResult.AddErrorBadRequest(nameof(EnumUnitErrorCode.UnitUsed), nameof(request.Id), request.Id);
                return methodResult;
            }
            _mapper.Map(request, unit);
            var method = await _unitHelper.Validate(unit, request);
            if (!method.IsOK)
            {
                methodResult.AddErrorBadRequest(method.ErrorMessages);
                return methodResult;
            }

            #endregion Validation

            await _unitRepository.ExecuteTransactionAsync(async () =>
            {
                unit.UnitLessons = request.LessonIds!.Select((x, index) => new UnitLesson
                {
                    DisplayOrder = index + 1,
                    LessonId = x
                }).ToList();
                if (request.MockTestId != null)
                {
                    unit.UnitSkillMockTests = new List<UnitSkillMockTest>
                    {
                        new UnitSkillMockTest
                        {
                            MockTestId = request.MockTestId ?? default,
                        }
                    };
                }
                unit = _unitRepository.Update(unit);
                await _unitRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<UnitModel>(unit);
                return methodResult;
            });

            return methodResult;
        }
    }
}
