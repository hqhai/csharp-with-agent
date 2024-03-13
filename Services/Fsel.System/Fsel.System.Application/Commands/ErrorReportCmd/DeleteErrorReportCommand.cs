// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.ErrorReportCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.System.Domain.IRepositories;
    using global::System;
    using global::System.Threading.Tasks;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class DeleteErrorReportCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }

    public class DeleteErrorReportCommandHandler : IRequestHandler<DeleteErrorReportCommand, MethodResult<bool>>
    {
        private readonly IErrorReportRepository _errorReportRepository;

        public DeleteErrorReportCommandHandler(IErrorReportRepository errorReportRepository)
        {
            _errorReportRepository = errorReportRepository;
        }

        public async Task<MethodResult<bool>> Handle(DeleteErrorReportCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            #region Validation

            var errorReport = await _errorReportRepository.GetByIdAsync(request.Id);
            if (errorReport == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(errorReport));
                return methodResult;
            }

            #endregion Validation

            await _errorReportRepository.ExecuteTransactionAsync(async () =>
            {
                var result = await _errorReportRepository.DeleteAsync(errorReport);
                await _errorReportRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = result;
                return methodResult;
            });

            return methodResult;
        }
    }
}
