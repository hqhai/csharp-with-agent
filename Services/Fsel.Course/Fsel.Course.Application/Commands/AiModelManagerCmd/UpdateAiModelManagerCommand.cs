// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.AiModelManagerCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.AiModelManager;
    using Fsel.Course.Domain.Models.EntityModels.AiManagerModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateAiModelManagerCommand : UpdateAiModelCommandManagerModel, IRequest<MethodResult<AiManagerModel>>
    {
    }

    public class UpdateAiModelManagerCommandHanlder : IRequestHandler<UpdateAiModelManagerCommand, MethodResult<AiManagerModel>>
    {
        private readonly IAiModelManagerRepository _aiModelManagerRepository;
        private readonly IMapper _mapper;

        public UpdateAiModelManagerCommandHanlder(IAiModelManagerRepository aiModelManagerRepository, IMapper mapper)
        {
            _aiModelManagerRepository = aiModelManagerRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<AiManagerModel>> Handle(UpdateAiModelManagerCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<AiManagerModel> methodResult = new MethodResult<AiManagerModel>();

            var exits = await _aiModelManagerRepository.GetByIdAsync(request.Id);
            if (exits == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            Validation(request, methodResult);

            await _aiModelManagerRepository.ExecuteTransactionAsync(async () =>
            {
                _mapper.Map(request, exits);
                if (!exits.IsValid())
                {
                    methodResult.AddErrorBadRequest(exits.ErrorMessages);
                    return methodResult;
                }

                exits = _aiModelManagerRepository.Update(exits);
                await _aiModelManagerRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<AiManagerModel>(exits);
                return methodResult;
            });

            return methodResult;
        }

        private MethodResult<AiManagerModel> Validation(UpdateAiModelManagerCommand request, MethodResult<AiManagerModel> methodResult)
        {
            if (request.AiModelName == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required));
                return methodResult;
            }

            if (request.InputModel == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required));
                return methodResult;
            }

            return methodResult;
        }
    }
}
