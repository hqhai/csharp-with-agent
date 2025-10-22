// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.AiCriteriaConfigCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.AiCriteriaConfig;
    using Fsel.Course.Domain.Models.EntityModels.AiPromptManagerModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class CreateAiCriteriaConfigCommand : CreateAiCriteriaConfigCommandModel, IRequest<MethodResult<AICriteriaConfigsModel>>
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
            MethodResult<AICriteriaConfigsModel> methodResult = new MethodResult<AICriteriaConfigsModel>();

            var modelExits = await _aiPromptManagerRepository.GetByIdAsync(request.AiPromptManagerId);

            if (modelExits == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            Validation(methodResult, request);

            await _aiCriteriaConfigRepository.ExecuteTransactionAsync(async () =>
            {
                var entity = _mapper.Map<AICriteriaConfigs>(request);
                entity = _aiCriteriaConfigRepository.Add(entity);

                await _aiCriteriaConfigRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken)
                .ConfigureAwait(false);

                methodResult.Result = _mapper.Map<AICriteriaConfigsModel>(entity);
                methodResult.StatusCode = StatusCodes.Status200OK;

                return methodResult;
            });

            return methodResult;
        }

        private static MethodResult<AICriteriaConfigsModel> Validation(MethodResult<AICriteriaConfigsModel> methodResult, CreateAiCriteriaConfigCommand request)
        {
            if (request.UserRole == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required));
                return methodResult;
            }

            if (request.SettingAiConfig == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required));
                return methodResult;
            }

            if (request.JsonConfig == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required));
                return methodResult;
            }

            return methodResult;
        }
    }
}
