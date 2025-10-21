// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.AiModelFeatureQuery
{
    using System.Net.WebSockets;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.AiModelFeature;
    using Fsel.Course.Domain.Models.EntityModels.AiManagerModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetAiModelFeatureQuery : IRequest<MethodResult<AiFeatureConfigModel>>
    {
        public EnumFeature Key { get; set; }
        public Guid AiModelManagerId { get; set; }
        public bool IncludeChildren { get; set; } = true;
    }

    public class GetAiModelFeatureQueryHandler : IRequestHandler<GetAiModelFeatureQuery, MethodResult<AiFeatureConfigModel>>
    {
        private readonly IAiFeatureConfigRepository _aiModelFeatureRepository;
        private readonly IMapper _mapper;

        public GetAiModelFeatureQueryHandler(IAiFeatureConfigRepository aiModelFeatureRepository, IMapper mapper)
        {
            _aiModelFeatureRepository = aiModelFeatureRepository;
            _mapper = mapper;
        }
        public async Task<MethodResult<AiFeatureConfigModel>> Handle(GetAiModelFeatureQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<AiFeatureConfigModel> methodResult = new MethodResult<AiFeatureConfigModel>();

            var aiFeature = await _aiModelFeatureRepository.Queryable
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.FeatureAi == request.Key && x.ParentFeatureId == null && !x.IsDeleted, cancellationToken);
            if (aiFeature == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(aiFeature));
                return methodResult;
            }

            var subFeatue = new List<AiSubFeatueModel>();

            if (aiFeature.ParentFeatureId == null && request.IncludeChildren)
            {
                FeatureModel.FeatureTypes.TryGetValue(request.Key, out var subType);
                subType ??= Array.Empty<EnumTypeFeatureAi>();

                var results = await _aiModelFeatureRepository.Queryable.AsNoTracking()
                    .Where(x => x.ParentFeatureId == aiFeature.Id && !x.IsDeleted)
                    .ToListAsync(cancellationToken);

                foreach (var item in subType)
                {
                    var result = results.FirstOrDefault(x => x.TypeFeatureAi == item);
                    if (result != null)
                    {
                        var subSettings =
                            new SetiingAiFeatureModel
                            (
                                aiFeature.SettingFrequency,
                                aiFeature.SettingTemperature,
                                aiFeature.SettingPresence,
                                aiFeature.SettingTopP,
                                aiFeature.SettingWordMaxLength,
                                aiFeature.MaximumNumber,
                                aiFeature.MaximumToken
                            );
                        subFeatue.Add(new AiSubFeatueModel(item, result.UserRole, result.AiConfigSetting, result.Json, subSettings));
                    }
                }
            }

            var resultMap = _mapper.Map<AiFeatureConfigModel>(aiFeature);
            resultMap.SubFeatures = subFeatue;

            methodResult.Result = resultMap;
            methodResult.StatusCode = StatusCodes.Status200OK;

            return methodResult;
        }
    }
}
