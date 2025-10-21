// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.AiModelFeatureCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.AiModelFeature;
    using Fsel.Course.Domain.Models.EntityModels.AiManagerModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateSettingAiModelFeature : UpdateSettingAiFeatureCommandModel, IRequest<MethodResult<AiFeatureConfigModel>>
    {
    }

    public class UpdateSettingAiModelFeatureHandler : IRequestHandler<UpdateSettingAiModelFeature, MethodResult<AiFeatureConfigModel>>
    {
        private readonly IAiFeatureConfigRepository _aiModelFeatureRepository;
        private readonly IMapper _mapper;

        public UpdateSettingAiModelFeatureHandler(IAiFeatureConfigRepository aiModelFeatureRepository, IMapper mapper)
        {
            _aiModelFeatureRepository = aiModelFeatureRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<AiFeatureConfigModel>> Handle(UpdateSettingAiModelFeature request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<AiFeatureConfigModel> methodResult = new MethodResult<AiFeatureConfigModel>();

            var exits = await _aiModelFeatureRepository.Queryable.AsNoTracking()
                .FirstOrDefaultAsync(x => x.FeatureAi == request.Key && x.ParentFeatureId == null && !x.IsDeleted, cancellationToken);

            if (exits == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            await _aiModelFeatureRepository.ExecuteTransactionAsync(async () =>
            {
                if (!exits.IsValid())
                {
                    methodResult.AddErrorBadRequest(exits.ErrorMessages);
                    return methodResult;
                }

                exits.SettingTemperature = request.SettingTemperature;
                exits.SettingWordMaxLength = request.SettingWordMaxLength;
                exits.SettingTopP = request.SettingTopP;
                exits.SettingFrequency = request.SettingFrequency;
                exits.SettingPresence = request.SettingPresence;
                if (request.MaximumNumber != null && request.MaximumToken != null && exits.FeatureAi == EnumFeature.AiPracticeGym)
                {
                    exits.MaximumNumber = request.MaximumNumber;
                    exits.MaximumToken = request.MaximumToken;
                }

                exits = _aiModelFeatureRepository.Update(exits);

                if (exits.SubFeatures != null)
                {
                    var subFeature = await _aiModelFeatureRepository.Queryable
                    .Where(x => x.ParentFeatureId == exits.Id && !x.IsDeleted)
                    .ToListAsync(cancellationToken);

                    foreach (var sub in subFeature)
                    {
                        sub.SettingTemperature = request.SettingTemperature;
                        sub.SettingWordMaxLength = request.SettingWordMaxLength;
                        sub.SettingTopP = request.SettingTopP;
                        sub.SettingFrequency = request.SettingFrequency;
                        sub.SettingPresence = request.SettingPresence;

                        _aiModelFeatureRepository.Update(sub);
                    }
                }

                await _aiModelFeatureRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                var ready = await _aiModelFeatureRepository.Queryable
                .Include(x => x.SubFeatures)
                .SingleAsync(x => x.Id == exits.Id, cancellationToken);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<AiFeatureConfigModel>(ready);
                return methodResult;
            });

            return methodResult;
        }
    }
}
