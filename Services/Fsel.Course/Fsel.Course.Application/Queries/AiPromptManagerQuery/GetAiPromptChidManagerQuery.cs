// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.AiPromptManagerQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels.AiPromptManagerModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetAiPromptChidManagerQuery : IRequest<MethodResult<IList<AiPromptManagerModel>>>
    {
        public Guid Id { get; set; }
    }

    public class GetAiPromptChidManagerQueryHandler : IRequestHandler<GetAiPromptChidManagerQuery, MethodResult<IList<AiPromptManagerModel>>>
    {
        private readonly IAiPromptManagerRepository _aiPromptManagerRepository;

        public GetAiPromptChidManagerQueryHandler(IAiPromptManagerRepository aiPromptManagerRepository)
        {
            _aiPromptManagerRepository = aiPromptManagerRepository;
        }

        public async Task<MethodResult<IList<AiPromptManagerModel>>> Handle(GetAiPromptChidManagerQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<AiPromptManagerModel>> methodResult = new MethodResult<IList<AiPromptManagerModel>>();

            var result = await _aiPromptManagerRepository.Queryable
                .Where(x => x.ParentId == request.Id && !x.IsDeleted)
                .AsNoTracking()
                .Select(x => new AiPromptManagerModel
                {
                    Id = x.Id,
                    AiModelName = x.AiModelName,
                    InputModel = x.InputModel,
                    FeatureAi = x.FeatureAi,
                    FeatureObjectId = x.FeatureObjectId,
                    ParentId = x.ParentId,
                })
                .ToListAsync(cancellationToken);

            if (result == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            methodResult.Result = result;
            methodResult.StatusCode = StatusCodes.Status200OK;

            return methodResult;
        }
    }
}
