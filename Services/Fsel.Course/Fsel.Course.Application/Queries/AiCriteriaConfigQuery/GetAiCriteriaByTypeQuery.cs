// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.AiCriteriaConfigQuery
{
    using AutoMapper;
    using Common.ActionResults;
    using Common.Enums.ErrorCodes;
    using Domain.Enums;
    using Domain.IRepositories;
    using Domain.Models.EntityModels.AiPromptManagerModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetAiCriteriaByTypeQuery : IRequest<MethodResult<AICriteriaConfigsModel>>
    {
        public EnumSubFeatureType SubFeatureType { get; set; }
    }

    public class GetAiCriteriaByTypeQueryHandler : IRequestHandler<GetAiCriteriaByTypeQuery, MethodResult<AICriteriaConfigsModel>>
    {
        private readonly IAiCriteriaConfigRepository _aiCriteriaConfigRepository;
        private readonly IMapper _mapper;

        public GetAiCriteriaByTypeQueryHandler(IAiCriteriaConfigRepository aiCriteriaConfigRepository, IMapper mapper)
        {
            _aiCriteriaConfigRepository = aiCriteriaConfigRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<AICriteriaConfigsModel>> Handle(GetAiCriteriaByTypeQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<AICriteriaConfigsModel>();

            var aiCriteria = await _aiCriteriaConfigRepository.ReadQueryable
                .Where(x => x.SubFeatureType == request.SubFeatureType && x.ObjectId == null && x.DefaultType == EnumDefaultType.Default)
                .ToListAsync(cancellationToken);

            if (aiCriteria.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(aiCriteria));
                return methodResult;
            }

            var result = _mapper.Map<AICriteriaConfigsModel>(aiCriteria.FirstOrDefault());
            result.AiCriteriaModel = _mapper.Map<IList<AiCriteriaModel>>(aiCriteria);

            methodResult.Result = result;
            methodResult.StatusCode = StatusCodes.Status200OK;

            return methodResult;
        }
    }
}
