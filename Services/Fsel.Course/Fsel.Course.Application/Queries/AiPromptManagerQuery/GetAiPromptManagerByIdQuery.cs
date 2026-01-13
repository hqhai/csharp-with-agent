// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.AiPromptManagerQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels.AiPromptManagerModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetAiPromptManagerByIdQuery : IRequest<MethodResult<AiPromptManagerModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetAiModelManagerByIdQueryHandler : IRequestHandler<GetAiPromptManagerByIdQuery, MethodResult<AiPromptManagerModel>>
    {
        private readonly IAiPromptManagerRepository _aiModelManagerRepository;
        private readonly IMapper _mapper;

        public GetAiModelManagerByIdQueryHandler(IAiPromptManagerRepository aiModelManagerRepository, IMapper mapper)
        {
            _aiModelManagerRepository = aiModelManagerRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<AiPromptManagerModel>> Handle(GetAiPromptManagerByIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<AiPromptManagerModel> methodResult = new MethodResult<AiPromptManagerModel>();

            var aiModelManager = await _aiModelManagerRepository.GetByIdAsync(request.Id);

            if (aiModelManager == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(aiModelManager));
                return methodResult;
            }

            methodResult.Result = _mapper.Map<AiPromptManagerModel>(aiModelManager);
            methodResult.StatusCode = StatusCodes.Status200OK;

            return methodResult;
        }
    }
}
