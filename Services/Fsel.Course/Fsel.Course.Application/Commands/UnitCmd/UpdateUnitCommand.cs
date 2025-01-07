// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Common.Enums.ErrorCodes;
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

            var isUnitUsed = await _unitRepository.IsUnitUsed(request.Id);
            if (isUnitUsed)
            {
                methodResult.AddErrorBadRequest(nameof(EnumUnitErrorCode.UnitUsed), nameof(request.Id), request.Id);
                return methodResult;
            }

            var method = await _unitHelper.Validate(request);
            if (!method.IsOK)
            {
                methodResult.AddErrorBadRequest(method.ErrorMessages);
                return methodResult;
            }

            #endregion Validation

            var unit = await _unitRepository.Queryable
                                  .Include(e => e.UnitLessons)
                                  .Include(e => e.UnitSkillMockTests)
                                  .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken: cancellationToken);
            if (unit == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(unit));
                return methodResult;
            }
            _mapper.Map(request, unit);
            _unitHelper.SetUnitData(unit, request);
            if (!unit.IsValid())
            {
                methodResult.AddErrorBadRequest(unit.ErrorMessages);
                return methodResult;
            }

            await _unitRepository.ExecuteTransactionAsync(async () =>
            {
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
