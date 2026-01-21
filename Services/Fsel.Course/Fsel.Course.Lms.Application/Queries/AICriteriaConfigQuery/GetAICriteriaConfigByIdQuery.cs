// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.AICriteriaConfigQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Models.EntityModels.AiPromptManagerModels;
    using Fsel.Course.Lms.Application.Services.AIConfigService;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetAICriteriaConfigByIdQuery : IRequest<MethodResult<AICriteriaConfigsModel>>
    {
        public Guid ObjectId { get; set; }
        public EnumSubFeatureType SubFeatureType { get; set; }
        public EnumFeatureMultiple FeatureMultiple { get; set; }
    }

    public class GetAICriteriaConfigByIdQueryHandler : IRequestHandler<GetAICriteriaConfigByIdQuery, MethodResult<AICriteriaConfigsModel>>
    {
        private readonly IMapper _mapper;
        private readonly IAIConfigSubmitService _aIConfigSubmitService;

        public GetAICriteriaConfigByIdQueryHandler(IMapper mapper,
            IAIConfigSubmitService aIConfigSubmitService)
        {
            _mapper = mapper;
            _aIConfigSubmitService = aIConfigSubmitService;
        }

        public async Task<MethodResult<AICriteriaConfigsModel>> Handle(GetAICriteriaConfigByIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<AICriteriaConfigsModel>();
            var aiCriteria = await _aIConfigSubmitService.FindAIConfigByIdAsync(request.ObjectId, request.SubFeatureType, request.FeatureMultiple, cancellationToken);
            var result = _mapper.Map<AICriteriaConfigsModel>(aiCriteria);
            if (result.AiCriteriaModels == null)
            {
                result.AiCriteriaModels = new List<AiCriteriaModel>
                {
                    _mapper.Map<AiCriteriaModel>(aiCriteria)
                };
            }
            methodResult.Result = result;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
