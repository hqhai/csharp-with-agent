// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.AiPromptManagerCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.AiPromptManager;
    using Fsel.Course.Domain.Models.EntityModels.AiPromptManagerModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class UpdateAiPromptManagerCommand : UpdateAiPromptCommandManagerModel, IRequest<MethodResult<AiPromptManagerModel>>
    {
    }

    public class UpdateAiPromptManagerCommandHanlder : IRequestHandler<UpdateAiPromptManagerCommand, MethodResult<AiPromptManagerModel>>
    {
        private readonly IAiPromptManagerRepository _aiModelManagerRepository;
        private readonly IMapper _mapper;

        public UpdateAiPromptManagerCommandHanlder(IAiPromptManagerRepository aiModelManagerRepository, IMapper mapper)
        {
            _aiModelManagerRepository = aiModelManagerRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<AiPromptManagerModel>> Handle(UpdateAiPromptManagerCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<AiPromptManagerModel> methodResult = new MethodResult<AiPromptManagerModel>();

            var exits = await _aiModelManagerRepository.GetByIdAsync(request.Id);

            if (exits == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            Validation(request, methodResult);

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

        private static MethodResult<AiPromptManagerModel> Validation(UpdateAiPromptManagerCommand request, MethodResult<AiPromptManagerModel> methodResult)
        {
            if (request.AiModelName == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required));
                return methodResult;
            }

            if (request.InputModel == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required));
                return methodResult;
            }

            return methodResult;
        }
    }
}
