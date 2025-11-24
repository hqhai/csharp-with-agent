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

    public class CreateAiCriteriaConfigCommand : IRequest<MethodResult<AICriteriaConfigsModel>>
    {
        public Guid AiPromptManagerId { get; set; }
        public CreateAiCriteriaConfigCommandModel? AiCriteriaConfig { get; set; }
        public IList<CreateAiCriteriaConfigCommandModel>? AiCriteriaConfigs { get; set; }
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

            await _aiCriteriaConfigRepository.ExecuteTransactionAsync(async () =>
            {
                var entities = _mapper.Map<List<AICriteriaConfigs>>(items);
                await _aiCriteriaConfigRepository.AddList(entities);
                await _aiCriteriaConfigRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken)
                    .ConfigureAwait(false);
                var result = new AICriteriaConfigsModel
                {
                    AiPromptManagerId    = request.AiPromptManagerId,
                    AiCriteriaModel      = _mapper.Map<IList<AiCriteriaModel>>(entities)
                };
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
    }
}
