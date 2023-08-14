// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.ForbiddenWordCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.CommandModels.ForbiddenWords;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System;
    using global::System.Globalization;
    using global::System.Threading.Tasks;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

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

            var allForbiddenWords = await _forbiddenWordRepository.Queryable.ToListAsync(cancellationToken);
            var forbiddenWordName = allForbiddenWords.Any(f => string.Equals(f.Word, request.Word, StringComparison.OrdinalIgnoreCase));

            if (forbiddenWordName)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(forbiddenWordName));
                return methodResult;
            }

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
