// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.ForbiddenWordCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.CommandModels.ForbiddenWords;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System;
    using global::System.Threading.Tasks;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class CreateForbiddenWordCommand : CreateForbiddenWordCommandModel, IRequest<MethodResult<ForbiddenWordModel>>
    {
    }

    public class CreateForbiddenWordCommandHandler : IRequestHandler<CreateForbiddenWordCommand, MethodResult<ForbiddenWordModel>>
    {
        private readonly IMapper _mapper;
        private readonly IForbiddenWordRepository _forbiddenWordRepository;

        public CreateForbiddenWordCommandHandler(IMapper mapper, IForbiddenWordRepository forbiddenWordRepository)
        {
            _mapper = mapper;
            _forbiddenWordRepository = forbiddenWordRepository;
        }

        public async Task<MethodResult<ForbiddenWordModel>> Handle(CreateForbiddenWordCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ForbiddenWordModel> methodResult = new MethodResult<ForbiddenWordModel>();

            ForbiddenWord forbiddenWord = _mapper.Map<ForbiddenWord>(request);
            await _forbiddenWordRepository.ExecuteTransactionAsync(async () =>
            {
                forbiddenWord = _forbiddenWordRepository.Add(forbiddenWord);
                await _forbiddenWordRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<ForbiddenWordModel>(forbiddenWord);
                return methodResult;
            });

            return methodResult;
        }
    }
}
