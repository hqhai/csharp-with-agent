// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.AiPromptManagerCmd
{
    using AutoMapper;
    using Common.ActionResults;
    using Common.Enums.ErrorCodes;
    using Domain.IRepositories;
    using Domain.Models.CommandModels.AiPromptManager;
    using Domain.Models.EntityModels.AiPromptManagerModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class UpdateAiPromptManagerCommand : UpdateAiPromptCommandManagerModel, IRequest<MethodResult<AiPromptManagerModel>>
    {
    }

    public class UpdateAiPromptManagerCommandHandler : IRequestHandler<UpdateAiPromptManagerCommand, MethodResult<AiPromptManagerModel>>
    {
        private readonly IAiPromptManagerRepository _aiModelManagerRepository;
        private readonly IMapper _mapper;

        public UpdateAiPromptManagerCommandHandler(IAiPromptManagerRepository aiModelManagerRepository, IMapper mapper)
        {
            _aiModelManagerRepository = aiModelManagerRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<AiPromptManagerModel>> Handle(UpdateAiPromptManagerCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<AiPromptManagerModel>();

            var exits = await _aiModelManagerRepository.GetByIdAsync(request.Id);

            if (exits == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            #region  Validation

            if (request.AiModelName == null || request.InputModelJson == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required));
                return methodResult;
            }

            #endregion

            await _aiModelManagerRepository.ExecuteTransactionAsync(async () =>
            {
                _mapper.Map(request, exits);

                if (!exits.IsValid())
                {
                    methodResult.AddErrorBadRequest(exits.ErrorMessages);
                    return methodResult;
                }

                exits = _aiModelManagerRepository.Update(exits);
                await _aiModelManagerRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<AiPromptManagerModel>(exits);

                return methodResult;
            });

            return methodResult;
        }
    }
}
