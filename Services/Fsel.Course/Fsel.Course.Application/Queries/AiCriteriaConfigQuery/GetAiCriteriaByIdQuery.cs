// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.AiCriteriaConfigQuery
{
    using System;
    using System.Collections.Generic;
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

    public class GetAiCriteriaByIdQuery : IRequest<MethodResult<AICriteriaConfigsModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetAiCriteriaByIdQueryHandler : IRequestHandler<GetAiCriteriaByIdQuery, MethodResult<AICriteriaConfigsModel>>
    {
        private readonly IAiCriteriaConfigRepository _aiCriteriaConfigRepository;
        private readonly IMapper _mapper;

        public GetAiCriteriaByIdQueryHandler(IAiCriteriaConfigRepository aiCriteriaConfigRepository, IMapper mapper)
        {
            _aiCriteriaConfigRepository = aiCriteriaConfigRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<AICriteriaConfigsModel>> Handle(GetAiCriteriaByIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<AICriteriaConfigsModel>();

            var aiCriteria = await _aiCriteriaConfigRepository.ReadQueryable
                .Where(x => x.Id == request.Id)
                .ToListAsync(cancellationToken);

            if (aiCriteria == null && aiCriteria.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(aiCriteria));
                return methodResult;
            }

            var result = _mapper.Map<AICriteriaConfigsModel>(aiCriteria.FirstOrDefault());
            result.AiCriteriaModels = _mapper.Map<IList<AiCriteriaModel>>(aiCriteria);

            methodResult.Result = result;
            methodResult.StatusCode = StatusCodes.Status200OK;

            return methodResult;
        }
    }
}
