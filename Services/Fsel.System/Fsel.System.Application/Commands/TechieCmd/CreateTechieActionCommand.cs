// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.TechieCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.CommandModels.TeachingCosts;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class CreateTechieActionCommand : SaveTeachingCostCommandModel, IRequest<MethodResult<TeachingCostModel>>
    {
    }

    public class CreateTechieActionCommandHandler : IRequestHandler<CreateTechieActionCommand, MethodResult<TeachingCostModel>>
    {
        private readonly IMapper _mapper;
        private readonly ITeachingCostRepository _teachingCostRepository;

        public CreateTechieActionCommandHandler(IMapper mapper, ITeachingCostRepository teachingCostRepository)
        {
            _mapper = mapper;
            _teachingCostRepository = teachingCostRepository;
        }

        public async Task<MethodResult<TeachingCostModel>> Handle(CreateTechieActionCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<TeachingCostModel>();

            await _teachingCostRepository.ExecuteTransactionAsync(async () =>
            {
                var teachingCost = await _teachingCostRepository.GetByIdAsync(request.Id ?? default);

                if (teachingCost != null)
                {
                    _mapper.Map(request, teachingCost);
                    teachingCost = _teachingCostRepository.Update(teachingCost);
                }
                else
                {
                    teachingCost = _mapper.Map<TeachingCost>(request);
                    teachingCost = _teachingCostRepository.Add(teachingCost);
                }
                if (!teachingCost.IsValid())
                {
                    methodResult.AddErrorBadRequest(teachingCost.ErrorMessages);
                    return methodResult;
                }

                await _teachingCostRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<TeachingCostModel>(teachingCost);
                return methodResult;
            });

            return methodResult;
        }
    }
}
