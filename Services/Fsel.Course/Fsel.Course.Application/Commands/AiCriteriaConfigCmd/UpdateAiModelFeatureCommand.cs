// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.AiCriteriaConfigCmd
{
    using AutoMapper;
    using Common.ActionResults;
    using Common.Enums.ErrorCodes;
    using Domain.Entities;
    using Domain.IRepositories;
    using Domain.Models.CommandModels.AiCriteriaConfig;
    using Domain.Models.EntityModels.AiPromptManagerModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateAiModelFeatureCommand : UpdateAiCriteriaCommandModel, IRequest<MethodResult<AICriteriaConfigsModel>>
    {
    }

    public class UpdateAiModelFeatureCommandHandler : IRequestHandler<UpdateAiModelFeatureCommand, MethodResult<AICriteriaConfigsModel>>
    {
        private readonly IAiCriteriaConfigRepository _aiCriteriaConfigRepository;
        private readonly IAiPromptManagerRepository _aiPromptManagerRepository;
        private readonly IMapper _mapper;

        public UpdateAiModelFeatureCommandHandler(IAiCriteriaConfigRepository aiCriteriaConfigRepository,
            IMapper mapper,
            IAiPromptManagerRepository aiPromptManagerRepository)
        {
            _aiCriteriaConfigRepository = aiCriteriaConfigRepository;
            _mapper = mapper;
            _aiPromptManagerRepository = aiPromptManagerRepository;
        }

        public async Task<MethodResult<AICriteriaConfigsModel>> Handle(UpdateAiModelFeatureCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<AICriteriaConfigsModel>();
            var items = new List<UpdateAiCriteriaCommand>();

            if (request.AiCriteriaConfig != null)
            {
                items.Add(request.AiCriteriaConfig);
            }

            if (request.AiCriteriaConfigs is { Count: > 0 })
            {
                items.AddRange(request.AiCriteriaConfigs);
            }

            if (items.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required));
                return methodResult;
            }

            foreach (var item in items)
            {
                Validation(methodResult, item);
            }

            if (!methodResult.IsOK)
            {
                return methodResult;
            }
            var ids = items.Select(x => x.Id).Distinct().ToList();

            var existing = await _aiCriteriaConfigRepository.ReadQueryable
                .Where(x => ids.Contains(x.Id) && !x.IsDeleted)//&& x.SubFeatureType == request.SubFeatureType && x.FeatureMultiple == request.FeatureMultiple)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            if (existing.Count != ids.Count)
            {
                var missing = ids.Except(existing.Select(e => e.Id)).ToList();
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), string.Join(",", missing));
                return methodResult;
            }

            await _aiCriteriaConfigRepository.ExecuteTransactionAsync(async () =>
            {
                var updated = new List<AICriteriaConfigs>();
                foreach (var it in items)
                {
                    var entity = existing.First(e => e.Id == it.Id);
                    _mapper.Map(it, entity);
                    if (request.AiPromptManagerId != Guid.Empty)
                    {
                        var parent = await _aiPromptManagerRepository.GetByIdAsync(request.AiPromptManagerId);
                        if (parent == null)
                        {
                            methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.AiPromptManagerId));
                            return methodResult;
                        }
                        entity.AiPromptManagerId = request.AiPromptManagerId;
                    }

                    entity = _aiCriteriaConfigRepository.Update(entity);
                    updated.Add(entity);
                }
                await _aiCriteriaConfigRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                var result = new AICriteriaConfigsModel
                {
                    AiPromptManagerId = request.AiPromptManagerId,
                    AiCriteriaModel = _mapper.Map<IList<AiCriteriaModel>>(updated)
                };
                methodResult.Result = result;
                methodResult.StatusCode = StatusCodes.Status200OK;

                return methodResult;
            });

            return methodResult;

        }

        private static MethodResult<AICriteriaConfigsModel> Validation(MethodResult<AICriteriaConfigsModel> methodResult, UpdateAiCriteriaCommand request)
        {
            if (request.UserRole == null || request.SettingAiConfig == null || request.JsonConfig == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required));
            }

            return methodResult;
        }
    }
}
