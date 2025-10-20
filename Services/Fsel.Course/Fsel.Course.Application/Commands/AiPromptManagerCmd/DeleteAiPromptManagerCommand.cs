// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.AiModelManagerCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class DeleteAiPromptManagerCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }

    public class DeleteAiPromptManagerCommandHandler : IRequestHandler<DeleteAiPromptManagerCommand, MethodResult<bool>>
    {
        private readonly IAiPromptManagerRepository _aiModelManagerRepository;

        public DeleteAiPromptManagerCommandHandler(IAiPromptManagerRepository aiModelManagerRepository)
        {
            _aiModelManagerRepository = aiModelManagerRepository;
        }

        public async Task<MethodResult<bool>> Handle(DeleteAiPromptManagerCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            if (request.Id == Guid.Empty)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required), nameof(request.Id));
                return methodResult;
            }

            var entity = await _aiModelManagerRepository.GetIncludeByIdAsync(request.Id);
            await Validation(entity, methodResult, request);

            if (!methodResult.IsOK)
            {
                return methodResult;
            }

            await _aiModelManagerRepository.ExecuteTransactionAsync(async () =>
            {
                var result = await _aiModelManagerRepository.DeleteAsync(entity);
                await _aiModelManagerRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = result;

                return methodResult;
            });

            return methodResult;
        }

        private async Task<MethodResult<bool>> Validation(AiPromptManager entity,
            MethodResult<bool> methodResult,
            DeleteAiPromptManagerCommand request)
        {
            if (entity == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(entity));
                return methodResult;
            }

            if (!entity.IsValid())
            {
                methodResult.AddErrorBadRequest(entity.ErrorMessages);
                return methodResult;
            }

            var isFeature = await _aiModelManagerRepository.IsFeature(request.Id);

            if (isFeature)
            {
                methodResult.AddErrorBadRequest(nameof(EnumAiModelManagerErrorCode.IsFeature), nameof(request.Id), request.Id);
                return methodResult;
            }

            return methodResult;
        }
    }
}
