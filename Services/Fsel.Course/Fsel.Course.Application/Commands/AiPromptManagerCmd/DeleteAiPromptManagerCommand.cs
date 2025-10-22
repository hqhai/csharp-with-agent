// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.AiPromptManagerCmd
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
    using Microsoft.EntityFrameworkCore;

    public class DeleteAiPromptManagerCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }

    public class DeleteAiPromptManagerCommandHandler : IRequestHandler<DeleteAiPromptManagerCommand, MethodResult<bool>>
    {
        private readonly IAiPromptManagerRepository _aiPromptManagerRepository;

        public DeleteAiPromptManagerCommandHandler(IAiPromptManagerRepository aiModelManagerRepository)
        {
            _aiPromptManagerRepository = aiModelManagerRepository;
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

            var entity = await _aiPromptManagerRepository.GetIncludeByIdAsync(request.Id);


            if (entity == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(entity));
                return methodResult;
            }

            await Validation(entity, methodResult, request);

            if (!methodResult.IsOK)
            {
                return methodResult;
            }

            await _aiPromptManagerRepository.ExecuteTransactionAsync(async () =>
            {
                var result = await _aiPromptManagerRepository.DeleteAsync(entity);
                await _aiPromptManagerRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

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
            if (!entity.IsValid())
            {
                methodResult.AddErrorBadRequest(entity.ErrorMessages);
                return methodResult;
            }

            var isCriteria = await _aiPromptManagerRepository.Queryable
                .Include(x => x.AiPromptParent)
                .Include(x => x.AICriteriaConfigs)
                .AsNoTracking()
                .AllAsync(x => x.Id == entity.Id && x.AICriteriaConfigs.Count > 0 && x.AiPromptManagers.Count > 0);

            if (isCriteria)
            {
                methodResult.AddErrorBadRequest(nameof(EnumAiPromptManagerErrorCode.IsFeature), nameof(request.Id), request.Id);
                return methodResult;
            }

            return methodResult;
        }
    }
}
