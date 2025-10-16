// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.AiModelManagerCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.AiModelManager;
    using Fsel.Course.Domain.Models.EntityModels;
    using MailKit.Net.Smtp;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateAiModelManagerCommand : CreateAiModelManagerCommandModel, IRequest<MethodResult<AiManagerModel>>
    {

    }

    public class CreateAiModelManagerCommandHandler : IRequestHandler<CreateAiModelManagerCommand, MethodResult<AiManagerModel>>
    {
        private readonly IAiModelManagerRepository _aiModelManagerRepository;
        private readonly IMapper _mapper;

        public CreateAiModelManagerCommandHandler(IAiModelManagerRepository aiModelManagerRepository, IMapper mapper)
        {
            _aiModelManagerRepository = aiModelManagerRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<AiManagerModel>> Handle(CreateAiModelManagerCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<AiManagerModel> methodResult = new MethodResult<AiManagerModel>();
            await Validation(request, methodResult, cancellationToken);

            await _aiModelManagerRepository.ExecuteTransactionAsync(async () =>
            {
                var entity = _mapper.Map<AiModelManager>(request);
                entity = _aiModelManagerRepository.Add(entity);
                await _aiModelManagerRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.Result = _mapper.Map<AiManagerModel>(entity);
                methodResult.StatusCode = StatusCodes.Status200OK;

                return methodResult;
            });

            return methodResult;
        }

        private async Task<MethodResult<AiManagerModel>> Validation(CreateAiModelManagerCommand request, MethodResult<AiManagerModel> methodResult, CancellationToken cancellationToken)
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

            var exits = await _aiModelManagerRepository.Queryable.FirstOrDefaultAsync(x => x.AiModelName == request.AiModelName, cancellationToken);

            if (exits != null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist));
                return methodResult;
            }

            return methodResult;
        }
    }
}
