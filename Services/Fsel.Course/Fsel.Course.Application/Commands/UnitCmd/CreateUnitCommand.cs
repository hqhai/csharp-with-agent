// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.CommandModels.Units;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Course.Infrastructure.Common.UnitHelper;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Fsel.Course.Application.Commands.UnitCmd
{
    public class CreateUnitCommand : UpdateUnitCommandModel, IRequest<MethodResult<UnitModel>>
    {
    }

    public class CreateUnitCommandHandler : IRequestHandler<CreateUnitCommand, MethodResult<UnitModel>>
    {
        private readonly IUnitRepository _unitRepository;
        private readonly IMapper _mapper;

        public CreateUnitCommandHandler(IUnitRepository unitRepository, IMapper mapper)
        {
            _unitRepository = unitRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<UnitModel>> Handle(CreateUnitCommand request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<UnitModel>();
            var validation = await UnitCreateValidation.Create(request)
                                    .ValidateRequestData()
                                    .ValidateDuplicateUnit(_unitRepository);

            var validationResult = validation.GetResult();
            if (validationResult.ErrorMessages.Any())
            {
                methodResult.AddErrorBadRequest(validationResult.ErrorMessages);
                return methodResult;
            }

            var unit = UnitFactory.Create(request).Build();
            if (!unit.IsValid())
            {
                methodResult.AddErrorBadRequest(unit.ErrorMessages);
                return methodResult;
            }

            await _unitRepository.ExecuteTransactionAsync(async () =>
            {
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
