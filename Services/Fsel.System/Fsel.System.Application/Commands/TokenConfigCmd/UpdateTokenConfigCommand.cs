// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.TokenConfigCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.CommandModels.TokenConfigs;
    using Fsel.System.Infrastructure.Common;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateTokenConfigCommand : UpdateTokenConfigsCommandModel, IRequest<MethodResult<IList<TokenConfigModel>>>
    {
    }

    public class UpdateTokenConfigCommandHandler : IRequestHandler<UpdateTokenConfigCommand, MethodResult<IList<TokenConfigModel>>>
    {
        private readonly IMapper _mapper;
        private readonly ITokenConfigRepository _tokenConfigRepository;
        private readonly TokenConfigsConverter _tokenConfigsConverter;

        public UpdateTokenConfigCommandHandler(IMapper mapper, ITokenConfigRepository tokenConfigRepository, TokenConfigsConverter tokenConfigsConverter)
        {
            _mapper = mapper;
            _tokenConfigRepository = tokenConfigRepository;
            _tokenConfigsConverter = tokenConfigsConverter;
        }

        public async Task<MethodResult<IList<TokenConfigModel>>> Handle(UpdateTokenConfigCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<TokenConfigModel>>();
            if (request.TokenConfigs == null || !request.TokenConfigs.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.TokenConfigs));
                return methodResult;
            }
            if (request.TokenConfigs.Any(x => x.Id == Guid.Empty))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(request.TokenConfigs));
                return methodResult;
            }
            var tokenConfigIds = request.TokenConfigs.Select(x => x.Id).ToList();
            var tokenConfigs = await _tokenConfigRepository.Queryable.Where(x => tokenConfigIds.Contains(x.Id)).ToListAsync(cancellationToken);

            if (tokenConfigs == null || !tokenConfigs.Any() || tokenConfigs.Count != tokenConfigIds.Count)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(tokenConfigs));
                return methodResult;
            }
            foreach (var item in request.TokenConfigs)
            {
                var tokenConfig = tokenConfigs.FirstOrDefault(x => x.Id == item.Id);
                if (tokenConfig != null)
                {
                    var (configs, validateData) = _tokenConfigsConverter.GetTokenConfigs(item.Config, tokenConfig.Config, tokenConfig.Feature);
                    if (!validateData)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(tokenConfig));
                        return methodResult;
                    }
                    tokenConfig.Config = configs;
                }
            }

            await _tokenConfigRepository.ExecuteTransactionAsync(async () =>
            {
                _tokenConfigRepository.UpdateList(tokenConfigs);
                await _tokenConfigRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.Result = _mapper.Map<IList<TokenConfigModel>>(tokenConfigs);
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            });

            return methodResult;
        }
    }
}
