// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.ActionResults;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.CommandModels.Units;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Course.Infrastructure.Common.UnitHelper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Application.Commands.UnitCmd
{
    public class UpdateUnitCommand : UpdateUnitCommandModel, IRequest<MethodResult<UnitModel>>
    {
    }

    public class UpdateUnitCommandHandler : IRequestHandler<UpdateUnitCommand, MethodResult<UnitModel>>
    {
        private readonly IUnitRepository _unitRepository;

        public UpdateUnitCommandHandler(IUnitRepository unitTestRepository)
        {
            _unitRepository = unitTestRepository;
        }

        public async Task<MethodResult<UnitModel>> Handle(UpdateUnitCommand request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<UnitModel>();
            var validationResult = UnitCreateValidation.Create(request)
                .ValidateRequestData()
                .GetResult();

            if (validationResult.ErrorMessages.Any())
            {
                methodResult.AddErrorBadRequest(validationResult.ErrorMessages);
                return methodResult;
            }

            var unit = await _unitRepository.Queryable
                                  .Where(e => e.Id == request.Id)
                                  .Include(e => e.UnitModules)
                                  .FirstOrDefaultAsync(cancellationToken: cancellationToken);
            if (unit == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(unit));
                return methodResult;
            }

            var isUsedUnit = await _unitRepository.IsUsingByClient(request.Id);

            methodResult = await UnitUpdaterFactory.Instance
                .CreateUnitUpdater(isUsedUnit, request, _unitRepository)
                .UpdateUnitAsync<UnitModel>(unit);

            return methodResult;
        }
    }
}
