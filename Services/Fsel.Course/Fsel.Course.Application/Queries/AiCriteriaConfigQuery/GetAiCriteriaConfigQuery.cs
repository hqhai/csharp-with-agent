// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.AiCriteriaConfigQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Domain.Enums;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels.AiPromptManagerModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetAiCriteriaConfigQuery : IRequest<MethodResult<AICriteriaConfigsModel>>
    {
        public EnumSubFeatureType Type { get; set; }
        public Guid ProjectId { get; set; }

    }

    public class GetAiModelFeatureQueryHandler : IRequestHandler<GetAiCriteriaConfigQuery, MethodResult<AICriteriaConfigsModel>>
    {
        private readonly IAiCriteriaConfigRepository _aiCriteriaSettingRepository;
        private readonly IMapper _mapper;

        public GetAiModelFeatureQueryHandler(IAiCriteriaConfigRepository aiModelFeatureRepository,
            IMapper mapper,
            IAiCriteriaConfigRepository aiCriteriaSettingRepository)
        {
            _mapper = mapper;
            _aiCriteriaSettingRepository = aiCriteriaSettingRepository;
        }
        public async Task<MethodResult<AICriteriaConfigsModel>> Handle(GetAiCriteriaConfigQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<AICriteriaConfigsModel>();

            var setting = await _aiCriteriaSettingRepository.ReadQueryable
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.SubFeatureType == request.Type && x.ProjectId == request.ProjectId, cancellationToken);

            if (setting == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(setting));
                return methodResult;
            }

            var criteria = await _aiCriteriaSettingRepository.ReadQueryable
                .AsNoTracking()
                .Where(x => x.SubFeatureType == request.Type &&  x.ProjectId == request.ProjectId)
                .ToListAsync(cancellationToken);

            if (criteria.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(criteria));
                return methodResult;
            }

            var result = _mapper.Map<AICriteriaConfigsModel>(setting);
            result.AiCriteriaModel = _mapper.Map<IList<AiCriteriaModel>>(criteria);;

            methodResult.Result = result;
            methodResult.StatusCode = StatusCodes.Status200OK;

            return methodResult;
        }
    }
}
