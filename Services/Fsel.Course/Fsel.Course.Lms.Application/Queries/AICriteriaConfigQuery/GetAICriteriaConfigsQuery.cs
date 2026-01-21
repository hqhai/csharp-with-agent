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

    public class GetAICriteriaConfigsQuery : IRequest<MethodResult<IList<AICriteriaConfigsModel>>>
    {
        public IList<Guid>? ObjectIds { get; set; }
        public EnumSubFeatureType SubFeatureType { get; set; }
        public EnumFeatureMultiple FeatureMultiple { get; set; }
    }

    public class GetAICriteriaConfigsQueryHandler : IRequestHandler<GetAICriteriaConfigsQuery, MethodResult<IList<AICriteriaConfigsModel>>>
    {
        private readonly IMapper _mapper;
        private readonly IAIConfigSubmitService _aIConfigSubmitService;

        public GetAICriteriaConfigsQueryHandler(IMapper mapper,
            IAIConfigSubmitService aIConfigSubmitService)
        {
            _mapper = mapper;
            _aIConfigSubmitService = aIConfigSubmitService;
        }

        public async Task<MethodResult<IList<AICriteriaConfigsModel>>> Handle(GetAICriteriaConfigsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<AICriteriaConfigsModel>>();
            if (request.ObjectIds == null || !request.ObjectIds.Any())
            {
                return methodResult;
            }

            var aiCriterias = await _aIConfigSubmitService.FindAIConfigsAsync(request.ObjectIds, request.SubFeatureType, request.FeatureMultiple, cancellationToken);
            if (aiCriterias == null || !aiCriterias.Any())
            {
                return methodResult;
            }
            var results = new List<AICriteriaConfigsModel>(aiCriterias.Count);

            foreach (var aiCriteria in aiCriterias)
            {
                var model = _mapper.Map<AICriteriaConfigsModel>(aiCriteria);
                if (model.AiCriteriaModels == null)
                {
                    model.AiCriteriaModels = new List<AiCriteriaModel>
                    {
                        _mapper.Map<AiCriteriaModel>(aiCriteria)
                    };
                }

                results.Add(model);
            }

            methodResult.Result = results;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
