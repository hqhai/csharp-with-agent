// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.AiPromptManagerQuery
{
    using AutoMapper;
    using Common.ActionResults;
    using Common.Enums.ErrorCodes;
    using Domain.IRepositories;
    using Domain.Models.EntityModels.AiPromptManagerModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetAiPromptManagerByProjectQuery : IRequest<MethodResult<IList<AiPromptManagerModel>>>
    {
        public Guid ProjectId { get; set; }
    }

    public class GetAiPromptManagerByProjectQueryHandler : IRequestHandler<GetAiPromptManagerByProjectQuery, MethodResult<IList<AiPromptManagerModel>>>
    {
        private readonly IAiPromptManagerRepository _aiPromptManagerRepository;
        private readonly IMapper _mapper;

        public GetAiPromptManagerByProjectQueryHandler(IAiPromptManagerRepository aiPromptManagerRepository,  IMapper mapper)
        {
            _aiPromptManagerRepository = aiPromptManagerRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<AiPromptManagerModel>>> Handle(GetAiPromptManagerByProjectQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<AiPromptManagerModel>>();

            var aiPromptManager = await _aiPromptManagerRepository.ReadQueryable
                .AsNoTracking()
                .Where(x => x.ProjectId == request.ProjectId)
                .ToListAsync(cancellationToken);

            if (aiPromptManager.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            methodResult.Result = _mapper.Map<IList<AiPromptManagerModel>>(aiPromptManager);
            methodResult.StatusCode = StatusCodes.Status200OK;

            return methodResult;
        }
    }
}
