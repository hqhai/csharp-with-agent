// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.AiPromptManagerCmd
{
    using AutoMapper;
    using Common.ActionResults;
    using Common.Enums.ErrorCodes;
    using Domain.IRepositories;
    using Domain.Models.CommandModels.AiPromptManager;
    using Domain.Models.EntityModels.AiPromptManagerModels;
    using Fsel.Course.Infrastructure.Common.AiPromptManagerHelpers;
    using Fsel.Course.Infrastructure.Common.VadilatorHelper;
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
                .FirstOrDefaultAsync(x => x.Name == request.Name, cancellationToken);

            if (exits != null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist));
                return methodResult;
            }

            #region  Validation

            var validationErrors = ValidationHelper.CollectValidationErrors(
                (!string.IsNullOrWhiteSpace(request.Name), nameof(request.Name)),
                (request.AiModel != null, nameof(request.AiModel))
            );

            if (validationErrors.Length > 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required), validationErrors.ToString());
                return methodResult;
            }

            #endregion


            await _aiModelManagerRepository.ExecuteTransactionAsync(async () =>
            {
                // Use Factory to create entity with versioning
                var entity = AiPromptManagerFactory
                    .Create(request, _mapper)
                    .Build();

                entity = _aiModelManagerRepository.Add(entity);
                await _aiModelManagerRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.Result = _mapper.Map<AiPromptManagerModel>(entity);
                methodResult.StatusCode = StatusCodes.Status200OK;

                return methodResult;
            });

            return methodResult;
        }
    }
}
