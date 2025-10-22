// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.AiCriteriaConfigCmd
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

    public class UpdateAiModelFeatureCommand : UpdateAiCriteriaCommandModel, IRequest<MethodResult<AICriteriaConfigsModel>>
    {
    }

    public class UpdateAiModelFeatureCommandHandler : IRequestHandler<UpdateAiModelFeatureCommand, MethodResult<AICriteriaConfigsModel>>
    {
        private readonly IAiCriteriaConfigRepository _aiCriteriaConfigRepository;
        private readonly IMapper _mapper;

        public UpdateAiModelFeatureCommandHandler(IAiCriteriaConfigRepository aiCriteriaConfigRepository, IMapper mapper)
        {
            _aiCriteriaConfigRepository = aiCriteriaConfigRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<AICriteriaConfigsModel>> Handle(UpdateAiModelFeatureCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<AICriteriaConfigsModel> methodResult = new MethodResult<AICriteriaConfigsModel>();

            var exits = await _aiCriteriaConfigRepository.Queryable.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (exits == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            Validation(request, methodResult);

            await _aiCriteriaConfigRepository.ExecuteTransactionAsync(async () =>
            {
                _mapper.Map(request, exits);

                if (!exits.IsValid())
                {
                    methodResult.AddErrorBadRequest(exits.ErrorMessages);
                    return methodResult;
                }

                exits = _aiCriteriaConfigRepository.Update(exits);
                await _aiCriteriaConfigRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.Result = _mapper.Map<AICriteriaConfigsModel>(exits);
                methodResult.StatusCode = StatusCodes.Status200OK;

                return methodResult;
            });

            return methodResult;

        }

        private static MethodResult<AICriteriaConfigsModel> Validation(UpdateAiModelFeatureCommand request, MethodResult<AICriteriaConfigsModel> methodResult)
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
