// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.AiFeatureConfigCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.AiCriteriaConfig;
    using Fsel.Course.Domain.Models.EntityModels.AiPromptManagerModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateSettingAiModelFeature : UpdateSettingAiFeatureCommandModel, IRequest<MethodResult<AICriteriaConfigsModel>>
    {
    }

    public class UpdateSettingAiModelFeatureHandler : IRequestHandler<UpdateSettingAiModelFeature, MethodResult<AICriteriaConfigsModel>>
    {
        private readonly IAiCriteriaConfigRepository _aiModelFeatureRepository;
        private readonly IMapper _mapper;

        public UpdateSettingAiModelFeatureHandler(IAiCriteriaConfigRepository aiModelFeatureRepository, IMapper mapper)
        {
            _aiModelFeatureRepository = aiModelFeatureRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<AICriteriaConfigsModel>> Handle(UpdateSettingAiModelFeature request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<AICriteriaConfigsModel> methodResult = new MethodResult<AICriteriaConfigsModel>();

            var exits = await _aiModelFeatureRepository.Queryable.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);

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

                if (request.MaximumNumber != null && request.MaximumToken != null)
                {
                    exits.MaximumNumber = request.MaximumNumber;
                    exits.MaximumToken = request.MaximumToken;
                }

                exits = _aiModelFeatureRepository.Update(exits);

                await _aiModelFeatureRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<AICriteriaConfigsModel>(exits);
                return methodResult;
            });

            return methodResult;
        }
    }
}
