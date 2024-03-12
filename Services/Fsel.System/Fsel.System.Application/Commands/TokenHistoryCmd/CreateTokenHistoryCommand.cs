// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.TokenHistoryCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.CommandModels.TokenHistorys;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System;
    using global::System.Threading.Tasks;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateTokenHistoryCommand : CreateTokenHistoryCommandModel, IRequest<MethodResult<IList<TokenHistoryModel>>>
    {
    }

    public class CreateTokenHistoryCommandHandler : IRequestHandler<CreateTokenHistoryCommand, MethodResult<IList<TokenHistoryModel>>>
    {
        private readonly IMapper _mapper;
        private readonly ITokenHistoryRepository _tokenHistoryRepository;
        private readonly ITokenConfigRepository _tokenConfigRepository;

        public CreateTokenHistoryCommandHandler(IMapper mapper, ITokenHistoryRepository tokenHistoryRepository, ITokenConfigRepository tokenConfigRepository)
        {
            _mapper = mapper;
            _tokenHistoryRepository = tokenHistoryRepository;
            _tokenConfigRepository = tokenConfigRepository;
        }

        public async Task<MethodResult<IList<TokenHistoryModel>>> Handle(CreateTokenHistoryCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<TokenHistoryModel>> methodResult = new MethodResult<IList<TokenHistoryModel>>();
            if (request.TokenHistorys == null || request.TokenHistorys.Count == 0)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            var tokenHistorys = new List<TokenHistory>();
            var tokenConfigs = await _tokenConfigRepository.Queryable.Where(x => request.TokenHistorys.Select(x => x.Feature).Contains(x.Feature) && request.TokenHistorys.Select(x => x.Mission).Contains(x.Mission)).ToListAsync(cancellationToken);
            foreach (var item in request.TokenHistorys)
            {
                TokenHistory tokenHistory = _mapper.Map<TokenHistory>(item);
                if (item.Feature == EnumTokenFeature.MarketPlace)
                {
                    item.Mission = null;
                    item.TokenConfigId = null;
                }
                else
                {
                    var tokenConfig = tokenConfigs.FirstOrDefault(x => x.Feature == item.Feature && x.Mission == item.Mission);
                    if (tokenConfig != null)
                    {
                        tokenHistory.TokenConfigId = tokenConfig.Id;
                        if (!tokenHistory.IsValid())
                        {
                            methodResult.AddErrorBadRequest(tokenHistory.ErrorMessages);
                            return methodResult;
                        }
                    }
                }

                tokenHistorys.Add(tokenHistory);
            }
            await _tokenHistoryRepository.ExecuteTransactionAsync(async () =>
            {
                await _tokenHistoryRepository.AddList(tokenHistorys);
                await _tokenHistoryRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<IList<TokenHistoryModel>>(tokenHistorys);
                return methodResult;
            });

            return methodResult;
        }
    }
}
