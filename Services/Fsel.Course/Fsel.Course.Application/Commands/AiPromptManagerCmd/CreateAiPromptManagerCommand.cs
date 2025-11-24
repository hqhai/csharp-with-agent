// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.AiPromptManagerCmd
{
    using AutoMapper;
    using Common.ActionResults;
    using Common.Enums.ErrorCodes;
    using Domain.Entities;
    using Domain.IRepositories;
    using Domain.Models.CommandModels.AiPromptManager;
    using Domain.Models.EntityModels.AiPromptManagerModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateAiPromptManagerCommand : CreateAiPromptManagerCommandModel, IRequest<MethodResult<AiPromptManagerModel>>
    {

    }

    public class CreateAiPromptManagerCommandHandler : IRequestHandler<CreateAiPromptManagerCommand, MethodResult<AiPromptManagerModel>>
    {
        private readonly IAiPromptManagerRepository _aiModelManagerRepository;
        private readonly IMapper _mapper;

        public CreateAiPromptManagerCommandHandler(IAiPromptManagerRepository aiModelManagerRepository, IMapper mapper)
        {
            _aiModelManagerRepository = aiModelManagerRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<AiPromptManagerModel>> Handle(CreateAiPromptManagerCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<AiPromptManagerModel>();

            var exits = await _aiModelManagerRepository.Queryable
                .FirstOrDefaultAsync(x => x.AiModelName == request.AiModelName, cancellationToken);

            if (exits != null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist));
                return methodResult;
            }

            Validation(request, methodResult);

            await _aiModelManagerRepository.ExecuteTransactionAsync(async () =>
            {
                var entity = _mapper.Map<AiPromptManager>(request);
                entity = _aiModelManagerRepository.Add(entity);
                await _aiModelManagerRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.Result = _mapper.Map<AiPromptManagerModel>(entity);
                methodResult.StatusCode = StatusCodes.Status200OK;

                return methodResult;
            });

            return methodResult;
        }

        private static MethodResult<AiPromptManagerModel> Validation(CreateAiPromptManagerCommand request, MethodResult<AiPromptManagerModel> methodResult)
        {
            if (request.AiModelName == null || request.InputModelJson == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required));
            }

            return methodResult;
        }
    }
}
