// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.TokenHistoryCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.CommandModels.TokenHistorys;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System;
    using global::System.Threading.Tasks;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class CreateTokenHistoryCommand : CreateTokenHistoryCommandModel, IRequest<MethodResult<TokenHistoryModel>>
    {
    }

    public class CreateTokenHistoryCommandHandler : IRequestHandler<CreateTokenHistoryCommand, MethodResult<TokenHistoryModel>>
    {
        private readonly IMapper _mapper;
        private readonly ITokenHistoryRepository _tokenHistoryRepository;

        public CreateTokenHistoryCommandHandler(IMapper mapper, ITokenHistoryRepository tokenHistoryRepository)
        {
            _mapper = mapper;
            _tokenHistoryRepository = tokenHistoryRepository;
        }

        public async Task<MethodResult<TokenHistoryModel>> Handle(CreateTokenHistoryCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<TokenHistoryModel> methodResult = new MethodResult<TokenHistoryModel>();
            TokenHistory tokenHistory = _mapper.Map<TokenHistory>(request);
            await _tokenHistoryRepository.ExecuteTransactionAsync(async () =>
            {
                tokenHistory = _tokenHistoryRepository.Add(tokenHistory);
                await _tokenHistoryRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<TokenHistoryModel>(tokenHistory);
                return methodResult;
            });

            return methodResult;
        }
    }
}
