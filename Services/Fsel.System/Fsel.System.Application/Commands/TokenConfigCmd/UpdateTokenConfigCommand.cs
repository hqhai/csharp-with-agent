// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.TokenConfigCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.CommandModels.TokenConfigs;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class UpdateTokenConfigCommand : UpdateTokenConfigCommandModel, IRequest<MethodResult<TokenConfigModel>>
    {
    }

    public class UpdateTokenConfigCommandHandler : IRequestHandler<UpdateTokenConfigCommand, MethodResult<TokenConfigModel>>
    {
        private readonly IMapper _mapper;
        private readonly ITokenConfigRepository _tokenConfigRepository;

        public UpdateTokenConfigCommandHandler(IMapper mapper, ITokenConfigRepository tokenConfigRepository)
        {
            _mapper = mapper;
            _tokenConfigRepository = tokenConfigRepository;
        }

        public async Task<MethodResult<TokenConfigModel>> Handle(UpdateTokenConfigCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<TokenConfigModel> methodResult = new MethodResult<TokenConfigModel>();
            var tokenConfig = await _tokenConfigRepository.GetByIdAsync(request.Id);

            #region Validation

            if (tokenConfig == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(tokenConfig));
                return methodResult;
            }

            _mapper.Map(request, tokenConfig);

            #endregion Validation

            await _tokenConfigRepository.ExecuteTransactionAsync(async () =>
            {
                tokenConfig = _tokenConfigRepository.Update(tokenConfig);
                await _tokenConfigRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<TokenConfigModel>(tokenConfig);
                return methodResult;
            });

            return methodResult;
        }
    }
}
