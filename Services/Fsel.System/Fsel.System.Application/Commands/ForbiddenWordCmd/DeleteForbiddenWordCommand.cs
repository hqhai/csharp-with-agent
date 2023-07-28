// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.ForbiddenWordCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.System.Domain.IRepositories;
    using global::System;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class DeleteForbiddenWordCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }

    public class DeleteForbiddenWordCommandHandler : IRequestHandler<DeleteForbiddenWordCommand, MethodResult<bool>>
    {
        private readonly IForbiddenWordRepository _forbiddenWordRepository;

        public DeleteForbiddenWordCommandHandler(IForbiddenWordRepository forbiddenWordRepository)
        {
            _forbiddenWordRepository = forbiddenWordRepository;
        }

        public async Task<MethodResult<bool>> Handle(DeleteForbiddenWordCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            #region Validation

            var forbiddenWord = await _forbiddenWordRepository.GetByIdAsync(request.Id);
            if (forbiddenWord == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(forbiddenWord));
                return methodResult;
            }

            #endregion Validation

            await _forbiddenWordRepository.ExecuteTransactionAsync(async () =>
            {
                var result = await _forbiddenWordRepository.DeleteAsync(forbiddenWord);
                await _forbiddenWordRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = result;
                return methodResult;
            });

            return methodResult;
        }
    }
}
