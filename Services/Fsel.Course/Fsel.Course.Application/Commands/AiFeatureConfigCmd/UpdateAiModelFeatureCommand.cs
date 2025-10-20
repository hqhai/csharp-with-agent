// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.AiModelFeatureCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.AiModelFeature;
    using Fsel.Course.Domain.Models.EntityModels.AiManagerModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateAiModelFeatureCommand : UpdateAiFeatureCommandModel, IRequest<MethodResult<AiFeatureConfigModel>>
    {
    }

    public class UpdateAiModelFeatureCommandHandler : IRequestHandler<UpdateAiModelFeatureCommand, MethodResult<AiFeatureConfigModel>>
    {
        private readonly IAiFeatureConfigRepository _aiModelFeatureRepository;
        private readonly IMapper _mapper;

        public UpdateAiModelFeatureCommandHandler(IAiFeatureConfigRepository aiModelFeatureRepository, IMapper mapper)
        {
            _aiModelFeatureRepository = aiModelFeatureRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<AiFeatureConfigModel>> Handle(UpdateAiModelFeatureCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<AiFeatureConfigModel> methodResult = new MethodResult<AiFeatureConfigModel>();

            var exits = await _aiModelFeatureRepository.Queryable.AsNoTracking().FirstOrDefaultAsync(x => x.FeatureAi == request.Key && x.ParentFeatureId == null && !x.IsDeleted, cancellationToken);
            if (exits == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }


            await _aiModelFeatureRepository.ExecuteTransactionAsync(async () =>
            {
                if (request.SubFeature == null)
                {
                    _mapper.Map(request, exits);
                    if (!exits.IsValid())
                    {
                        methodResult.AddErrorBadRequest(exits.ErrorMessages);
                        return methodResult;
                    }

                    exits = _aiModelFeatureRepository.Update(exits);
                    await _aiModelFeatureRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                    methodResult.StatusCode = StatusCodes.Status200OK;
                    methodResult.Result = _mapper.Map<AiFeatureConfigModel>(exits);

                    return methodResult;
                }
                else
                {
                    ValidationSubFeature(request, methodResult);

                    foreach (var sub in request.SubFeature)
                    {
                        var subFeature = await _aiModelFeatureRepository.Queryable
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.FeatureAi == exits.FeatureAi && x.TypeFeatureAi == sub.TypeFeatureAi);

                        if (subFeature == null)
                        {
                            subFeature = new AIFeatureConfig
                            {
                                AiPromptManagerId = (Guid)request.AiPromptManagerId,
                                ParentFeatureId = exits.Id,
                                FeatureAi = exits.FeatureAi,
                                TypeFeatureAi = sub.TypeFeatureAi,
                                UserRole = sub.UserRole,
                                Config = sub.Config,
                                JsonConfig = sub.JsonConfig,
                            };
                            _aiModelFeatureRepository.Add(subFeature);
                            exits.SubFeatures.Add(subFeature);
                        }

                        subFeature.UserRole = sub.UserRole;
                        subFeature.Config = sub.Config;
                        subFeature.JsonConfig = sub.JsonConfig;

                        subFeature = _aiModelFeatureRepository.Update(subFeature);
                    }
                    await _aiModelFeatureRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                    var map = await _aiModelFeatureRepository.Queryable.Include(x => x.SubFeatures).SingleAsync(x => x.Id == exits.Id, cancellationToken);

                    methodResult.Result = _mapper.Map<AiFeatureConfigModel>(map);
                    methodResult.StatusCode = StatusCodes.Status200OK;

                    return methodResult;
                }

            });

            return methodResult;

        }

        private MethodResult<AiFeatureConfigModel> ValidationSubFeature(UpdateAiModelFeatureCommand request, MethodResult<AiFeatureConfigModel> methodResult)
        {
            if (request.SubFeature.Select(x => x.UserRole) == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required));
                return methodResult;
            }

            if (request.SubFeature.Select(x => x.Config) == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required));
                return methodResult;
            }

            if (request.SubFeature.Select(x => x.JsonConfig) == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required));
                return methodResult;
            }
            return methodResult;
        }
    }
}
