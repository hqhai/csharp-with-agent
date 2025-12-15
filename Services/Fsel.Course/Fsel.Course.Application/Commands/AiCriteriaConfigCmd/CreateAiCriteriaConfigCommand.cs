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

    public class CreateAiCriteriaConfigCommand : CreateOrUpdateAiCriteriaCommand, IRequest<MethodResult<AICriteriaConfigsModel>>
    {

    }

    public class CreateAiCriteriaConfigHasSubFeatureCommandHandler : IRequestHandler<CreateAiCriteriaConfigCommand, MethodResult<AICriteriaConfigsModel>>
    {
        private readonly IAiCriteriaConfigRepository _aiCriteriaConfigRepository;
        private readonly IAiPromptManagerRepository _aiPromptManagerRepository;
        private readonly IMapper _mapper;

        public CreateAiCriteriaConfigHasSubFeatureCommandHandler(IAiCriteriaConfigRepository aiCriteriaConfigRepository,
            IMapper mapper,
            IAiPromptManagerRepository aiPromptManagerRepository)
        {
            _aiCriteriaConfigRepository = aiCriteriaConfigRepository;
            _mapper = mapper;
            _aiPromptManagerRepository = aiPromptManagerRepository;
        }

        public async Task<MethodResult<AICriteriaConfigsModel>> Handle(CreateAiCriteriaConfigCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var methodResult = new MethodResult<AICriteriaConfigsModel>();

            var modelExits = await _aiPromptManagerRepository.GetByIdAsync(request.AiPromptManagerId);

            if (modelExits == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var items = new List<CreateAiCriteriaConfigCommandModel>();

            if (request.AiCriteriaModel != null)
            {
                items.Add(request.AiCriteriaModel);
            }

            if (request.AiCriteriaModels != null && request.AiCriteriaModels.Count > 0)
            {
                items.AddRange(request.AiCriteriaModels);
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

            var updateIds = items
               .Where(x => x.Id.HasValue && x.Id.Value != Guid.Empty)
               .Select(x => x.Id!.Value)
               .Distinct()
               .ToList();

            var existingEntities = new List<AICriteriaConfigs>();

            if (updateIds.Count > 0)
            {
                existingEntities = await _aiCriteriaConfigRepository.Queryable
                    .Where(x => updateIds.Contains(x.Id)
                                && x.AiPromptManagerId == request.AiPromptManagerId)
                    .ToListAsync(cancellationToken);
            }

            var toCreate = new List<AICriteriaConfigs>();
            var toUpdate = new List<AICriteriaConfigs>();

            foreach (var item in items)
            {
                var isCreate = !item.Id.HasValue || item.Id.Value == Guid.Empty;

                if (isCreate)
                {
                    var entity = BuildCreateEntity(item, request);
                    toCreate.Add(entity);
                }
                else
                {
                    var entity = existingEntities.FirstOrDefault(x => x.Id == item.Id!.Value);
                    if (entity == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                        continue;
                    }

                    ApplyUpdateEntity(item, entity, request);

                    toUpdate.Add(entity);
                }
            }
            if (!methodResult.IsOK)
            {
                return methodResult;
            }
            await _aiCriteriaConfigRepository.ExecuteTransactionAsync(async () =>
            {
                if (toCreate.Count > 0)
                {
                    await _aiCriteriaConfigRepository.AddList(toCreate);
                }
                if (toUpdate.Count > 0)
                {
                    _aiCriteriaConfigRepository.UpdateList(toUpdate);
                }
                await _aiCriteriaConfigRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken)
                    .ConfigureAwait(false);
                var resultEntities = toCreate.Concat(toUpdate).ToList();
                var result = _mapper.Map<AICriteriaConfigsModel>(resultEntities.FirstOrDefault());

                result.AiCriteriaModels = _mapper.Map<IList<AiCriteriaModel>>(resultEntities);
                result.AiPromptManagerId = request.AiPromptManagerId;

                methodResult.Result = result;
                methodResult.StatusCode = StatusCodes.Status200OK;

                return methodResult;
            });

            return methodResult;
        }

        private static MethodResult<AICriteriaConfigsModel> Validation(MethodResult<AICriteriaConfigsModel> methodResult, CreateAiCriteriaConfigCommandModel request)
        {
            if (request.UserRole == null || request.SettingAiConfig == null || request.JsonConfig == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required));
            }

            return methodResult;
        }
        private AICriteriaConfigs BuildCreateEntity(
            CreateAiCriteriaConfigCommandModel item,
            CreateAiCriteriaConfigCommand request)
        {
            var entity = _mapper.Map<AICriteriaConfigs>(item);
            _mapper.Map(request, entity);

            return entity;
        }

        private void ApplyUpdateEntity(
            CreateAiCriteriaConfigCommandModel item,
            AICriteriaConfigs entity,
            CreateAiCriteriaConfigCommand request)
        {
            _mapper.Map(item, entity);
            _mapper.Map(request, entity);
        }
    }
}
