// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.Products
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class ConfirmReceiptOfProductCommand : IRequest<MethodResult<bool>>
    {
        public string? Code { get; set; }
    }

    public class ConfirmReceiptOfProductCommandHandler : IRequestHandler<ConfirmReceiptOfProductCommand, MethodResult<bool>>
    {
        private readonly IOrderTransactionRepository _orderTransactionRepository;

        public ConfirmReceiptOfProductCommandHandler(IOrderTransactionRepository orderTransactionRepository)
        {
            _orderTransactionRepository = orderTransactionRepository;
        }

        public async Task<MethodResult<bool>> Handle(ConfirmReceiptOfProductCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            if (string.IsNullOrEmpty(request.Code))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required));
                return methodResult;
            }

            var transaction = await _orderTransactionRepository.Queryable.FirstOrDefaultAsync(p => p.Code == request.Code, cancellationToken);
            if (transaction == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            await _orderTransactionRepository.ExecuteTransactionAsync(async () =>
            {
                transaction.Status = EnumOrderTransactionStatus.Received;
                _orderTransactionRepository.Update(transaction);
                await _orderTransactionRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });
            return methodResult;
        }
    }
}
