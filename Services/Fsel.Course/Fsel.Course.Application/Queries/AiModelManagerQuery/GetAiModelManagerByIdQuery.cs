// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.AiModelManagerQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetAiModelManagerByIdQuery : IRequest<MethodResult<AiManagerModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetAiModelManagerByIdQueryHandler : IRequestHandler<GetAiModelManagerByIdQuery, MethodResult<AiManagerModel>>
    {
        private readonly IAiModelManagerRepository _aiModelManagerRepository;
        private readonly IMapper _mapper;

        public GetAiModelManagerByIdQueryHandler(IAiModelManagerRepository aiModelManagerRepository, IMapper mapper)
        {
            _aiModelManagerRepository = aiModelManagerRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<AiManagerModel>> Handle(GetAiModelManagerByIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<AiManagerModel> methodResult = new MethodResult<AiManagerModel>();

            var aiModelManager = await _aiModelManagerRepository.GetByIdAsync(request.Id);

            if (aiModelManager == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(aiModelManager));
                return methodResult;
            }

            methodResult.Result = _mapper.Map<AiManagerModel>(aiModelManager);
            methodResult.StatusCode = StatusCodes.Status200OK;

            return methodResult;
        }
    }
}
