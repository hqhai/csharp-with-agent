// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.ForbiddenWordCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.System.Domain.IRepositories;
    using global::System;
    using global::System.Collections.Generic;
    using global::System.Linq;
    using global::System.Text;
    using global::System.Threading.Tasks;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class DeleteListForbiddenWordCommand : IRequest<MethodResult<bool>>
    {
        public IList<Guid>? Ids { get; set; }
    }
    public class DeleteListForbiddenWordCommandHandler : IRequestHandler<DeleteListForbiddenWordCommand, MethodResult<bool>>
    {
        private readonly IForbiddenWordRepository _forbiddenWordRepository;

        public DeleteListForbiddenWordCommandHandler(IForbiddenWordRepository forbiddenWordRepository)
        {
            _forbiddenWordRepository = forbiddenWordRepository;
        }

        public async Task<MethodResult<bool>> Handle(DeleteListForbiddenWordCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            #region Validation

            var forbiddenWords = await _forbiddenWordRepository.Queryable.Where(x => request.Ids!.Contains(x.Id)).ToListAsync(cancellationToken);
            if (forbiddenWords == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(forbiddenWords));
                return methodResult;
            }

            #endregion Validation

            await _forbiddenWordRepository.ExecuteTransactionAsync(async () =>
            {
                var result = await _forbiddenWordRepository.DeleteListAsync(forbiddenWords);
                await _forbiddenWordRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = result;
                return methodResult;
            });

            return methodResult;
        }
    }
}
