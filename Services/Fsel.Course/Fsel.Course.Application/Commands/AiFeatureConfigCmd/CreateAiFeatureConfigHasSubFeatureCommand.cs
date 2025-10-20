// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.AiModelFeatureCmd
{
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
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateAiFeatureConfigHasSubFeatureCommand : CreateAiFeatureConfigCommandModel, IRequest<MethodResult<AiFeatureConfigModel>>
    {
    }

    public class CreateAiFeatureconfigHasSubFeatureCommandHandler : IRequestHandler<CreateAiFeatureConfigHasSubFeatureCommand, MethodResult<AiFeatureConfigModel>>
    {
        private readonly IAiFeatureConfigRepository _aiModelFeatureRepository;
        private readonly IAiPromptManagerRepository _aiModelManagerRepository;
        private readonly IMapper _mapper;

        public CreateAiFeatureconfigHasSubFeatureCommandHandler(IAiFeatureConfigRepository aiModelFeatureRepository,
            IMapper mapper,
            IAiPromptManagerRepository aiModelManagerRepository)
        {
            _aiModelFeatureRepository = aiModelFeatureRepository;
            _mapper = mapper;
            _aiModelManagerRepository = aiModelManagerRepository;
        }

        public async Task<MethodResult<AiFeatureConfigModel>> Handle(CreateAiFeatureConfigHasSubFeatureCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<AiFeatureConfigModel> methodResult = new MethodResult<AiFeatureConfigModel>();

            var modelExits = await _aiModelManagerRepository.GetByIdAsync(request.AiPromptManagerId);

            if (modelExits == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            await _aiModelFeatureRepository.ExecuteTransactionAsync(async () =>
            {

                var parent = await _aiModelFeatureRepository.Queryable.
                    SingleOrDefaultAsync(x => x.AiPromptManagerId == request.AiPromptManagerId
                    && x.FeatureAi == request.FeatureAi
                    && x.ParentFeatureId == null && x.TypeFeatureAi == null && !x.IsDeleted, cancellationToken);

                if (parent == null)
                {
                    parent = new AIFeatureConfig
                    {
                        Id = Guid.NewGuid(),
                        AiPromptManagerId = request.AiPromptManagerId,
                        FeatureAi = request.FeatureAi,
                        FeatureObjectId = request.FeatureObjectId,
                        UserRole = request.UserRole,
                        Config = request.Config,
                        JsonConfig = request.JsonConfig,
                    };
                    parent = _aiModelFeatureRepository.Add(parent);
                }


                FeatureModel.FeatureTypes.TryGetValue(request.FeatureAi, out var allowed);
                allowed ??= Array.Empty<EnumTypeFeatureAi>();
                var hasChildren = allowed.Length > 0;
                var incoming = hasChildren ? (request.SubFeature ?? new List<CreateAiFeatuerHasSubCommadModel>()) : new List<CreateAiFeatuerHasSubCommadModel>();

                var fillter = incoming.Where(x => allowed.Contains(x.TypeFeatureAi)).ToList();

                if (hasChildren && request.SubFeature != null)
                {
                    var exits = await _aiModelFeatureRepository.Queryable
                    .Where(x => x.ParentFeatureId == parent.Id && !x.IsDeleted)
                    .ToDictionaryAsync(x => x.TypeFeatureAi!.Value, cancellationToken);

                    var byType = request.SubFeature
                    .Where(s => allowed.Contains(s.TypeFeatureAi))
                    .GroupBy(s => s.TypeFeatureAi)
                    .ToDictionary(g => g.Key, g => g.Last());

                    foreach (var (type, s) in byType)
                    {
                        var hasAny = !string.IsNullOrWhiteSpace(s.UserRole)
                         || !string.IsNullOrWhiteSpace(s.Config)
                         || !string.IsNullOrWhiteSpace((string?)s.JsonConfig);

                        if (!hasAny)
                        {
                            continue;
                        }

                        if (!exits.TryGetValue(type, out var subFeature))
                        {
                            subFeature = new AIFeatureConfig
                            {
                                AiPromptManagerId = parent.AiPromptManagerId,
                                FeatureAi = parent.FeatureAi,
                                ParentFeatureId = parent.Id,
                                TypeFeatureAi = type
                            };
                            _aiModelFeatureRepository.Add(subFeature);
                            exits[type] = subFeature;
                        }
                        subFeature.UserRole = s.UserRole?.Trim();
                        subFeature.Config = s.Config?.Trim();
                        subFeature.JsonConfig = s.JsonConfig ?? subFeature.JsonConfig;

                        //parent.SubFeatures.Add(subFeature);
                    }
                }

                await _aiModelFeatureRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken)
                .ConfigureAwait(false);

                var parentReady = await _aiModelFeatureRepository.Queryable
                    .Include(x => x.SubFeatures)
                    .SingleAsync(x => x.Id == parent.Id, cancellationToken);

                methodResult.Result = _mapper.Map<AiFeatureConfigModel>(parentReady);
                methodResult.StatusCode = StatusCodes.Status200OK;

                return methodResult;
            });

            return methodResult;
        }
    }
}
