// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.AiModelManagerQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels.AiManagerModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetAiModelManagerByIdQuery : IRequest<MethodResult<AiPromptManagerModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetAiModelManagerByIdQueryHandler : IRequestHandler<GetAiModelManagerByIdQuery, MethodResult<AiPromptManagerModel>>
    {
        private readonly IAiPromptManagerRepository _aiModelManagerRepository;
        private readonly IMapper _mapper;

        public GetAiModelManagerByIdQueryHandler(IAiPromptManagerRepository aiModelManagerRepository, IMapper mapper)
        {
            _aiModelManagerRepository = aiModelManagerRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<AiPromptManagerModel>> Handle(GetAiModelManagerByIdQuery request, CancellationToken cancellationToken)
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
