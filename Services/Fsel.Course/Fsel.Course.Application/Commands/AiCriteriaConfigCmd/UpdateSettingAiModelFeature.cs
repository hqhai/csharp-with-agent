// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.AiCriteriaConfigCmd
{
    using AutoMapper;
    using Common.ActionResults;
    using Common.Enums.ErrorCodes;
    using Domain.IRepositories;
    using Domain.Models.CommandModels.AiCriteriaConfig;
    using Domain.Models.EntityModels.AiPromptManagerModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateSettingAiModelFeature : UpdateSettingAiFeatureCommandModel, IRequest<MethodResult<AICriteriaConfigsModel>>
    {
    }

    public class UpdateSettingAiModelFeatureHandler : IRequestHandler<UpdateSettingAiModelFeature, MethodResult<AICriteriaConfigsModel>>
    {
        private readonly IAiCriteriaConfigRepository _aiCriteriaConfigRepository;
        private readonly IMapper _mapper;

        public UpdateSettingAiModelFeatureHandler(IAiCriteriaConfigRepository aiCriteriaConfigRepository, IMapper mapper)
        {
            _aiCriteriaConfigRepository = aiCriteriaConfigRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<AICriteriaConfigsModel>> Handle(UpdateSettingAiModelFeature request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<AICriteriaConfigsModel>();

            var existing = await _aiCriteriaConfigRepository.ReadQueryable
                .Where(x => x.ProjectId == request.ProjectId && x.SubFeatureType == request.SubFeatureType && !x.IsDeleted)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            if (existing.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            await _aiCriteriaConfigRepository.ExecuteTransactionAsync(async () =>
            {
                foreach (var item in existing)
                {
                    _mapper.Map(request, item);
                    _aiCriteriaConfigRepository.Update(item);
                }

                await _aiCriteriaConfigRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                var result = _mapper.Map<AICriteriaConfigsModel>(existing.First());
                result.AiCriteriaModels = _mapper.Map<IList<AiCriteriaModel>>(existing);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = result;

                return methodResult;
            });

            return methodResult;
        }
    }
}
