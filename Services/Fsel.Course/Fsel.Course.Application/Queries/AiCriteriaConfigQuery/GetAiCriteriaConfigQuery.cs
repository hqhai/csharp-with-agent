// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.AiPromptConfigQuery
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
    using Microsoft.EntityFrameworkCore;

    public class GetAiCriteriaConfigQuery : IRequest<MethodResult<AICriteriaConfigsModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetAiModelFeatureQueryHandler : IRequestHandler<GetAiCriteriaConfigQuery, MethodResult<AICriteriaConfigsModel>>
    {
        private readonly IAiCriteriaConfigRepository _aiModelFeatureRepository;
        private readonly IMapper _mapper;

        public GetAiModelFeatureQueryHandler(IAiCriteriaConfigRepository aiModelFeatureRepository, IMapper mapper)
        {
            _aiModelFeatureRepository = aiModelFeatureRepository;
            _mapper = mapper;
        }
        public async Task<MethodResult<AICriteriaConfigsModel>> Handle(GetAiCriteriaConfigQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<AICriteriaConfigsModel> methodResult = new MethodResult<AICriteriaConfigsModel>();

            var ctriteria = await _aiModelFeatureRepository.Queryable
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (ctriteria == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(ctriteria));
                return methodResult;
            }

            var resultMap = _mapper.Map<AICriteriaConfigsModel>(ctriteria);

            methodResult.Result = resultMap;
            methodResult.StatusCode = StatusCodes.Status200OK;

            return methodResult;
        }
    }
}
