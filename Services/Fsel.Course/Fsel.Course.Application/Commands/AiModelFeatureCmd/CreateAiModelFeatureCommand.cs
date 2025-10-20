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

    public class CreateAiModelFeatureCommand : CreateAiFeatureCommandModel, IRequest<MethodResult<AiFeatureModel>>
    {
    }

    public class CreateAiModelFeatureCommandHandler : IRequestHandler<CreateAiModelFeatureCommand, MethodResult<AiFeatureModel>>
    {
        private readonly IAiModelFeatureRepository _aiModelFeatureRepository;
        private readonly IMapper _mapper;

        public CreateAiModelFeatureCommandHandler(IAiModelFeatureRepository aiModelFeatureRepository, IMapper mapper)
        {
            _aiModelFeatureRepository = aiModelFeatureRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<AiFeatureModel>> Handle(CreateAiModelFeatureCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<AiFeatureModel> methodResult = new MethodResult<AiFeatureModel>();

            Validation(request, methodResult);

            await _aiModelFeatureRepository.ExecuteTransactionAsync(async () =>
            {
                var entity = _mapper.Map<AiModelFeature>(request);
                entity = _aiModelFeatureRepository.Add(entity);

                await _aiModelFeatureRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken)
                .ConfigureAwait(false);

                methodResult.Result = _mapper.Map<AiFeatureModel>(entity);
                methodResult.StatusCode = StatusCodes.Status200OK;

                return methodResult;
            });

            return methodResult;

        }

        private MethodResult<AiFeatureModel> Validation(CreateAiModelFeatureCommand request, MethodResult<AiFeatureModel> methodResult)
        {
            if (request.UserRole == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required));
                return methodResult;
            }

            if (request.Config == null)
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
