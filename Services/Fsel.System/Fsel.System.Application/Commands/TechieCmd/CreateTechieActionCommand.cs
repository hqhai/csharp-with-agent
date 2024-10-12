// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.TechieCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.CommandModels.Techie;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class CreateTechieActionCommand : SaveTechieActionCommandModel, IRequest<MethodResult<TechieActionModel>>
    {
    }

    public class CreateTechieActionCommandHandler : IRequestHandler<CreateTechieActionCommand, MethodResult<TechieActionModel>>
    {
        private readonly IMapper _mapper;
        private readonly ITechieActionRepository _techieActionRepository;

        public CreateTechieActionCommandHandler(IMapper mapper, ITechieActionRepository techieActionRepository)
        {
            _mapper = mapper;
            _techieActionRepository = techieActionRepository;
        }

        public async Task<MethodResult<TechieActionModel>> Handle(CreateTechieActionCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<TechieActionModel>();

            var techieAction = _mapper.Map<TechieAction>(request);
            await _techieActionRepository.ExecuteTransactionAsync(async () =>
            {
                _techieActionRepository.Add(techieAction);
                await _techieActionRepository.UnitOfWork.SaveEntitiesAsync().ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<TechieActionModel>(techieAction);
                return methodResult;
            });

            return methodResult;
        }
    }
}
