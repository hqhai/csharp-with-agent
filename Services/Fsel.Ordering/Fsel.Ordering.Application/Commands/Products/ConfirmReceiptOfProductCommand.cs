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
        public IList<string>? Codes { get; set; }
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

            if (request.Codes == null || request.Codes.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required));
                return methodResult;
            }

            var transactions = await _orderTransactionRepository.Queryable.Where(p => !string.IsNullOrEmpty(p.Code) && request.Codes.Contains(p.Code)).ToListAsync(cancellationToken);
            if (transactions == null || transactions.Count != request.Codes.Count)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            await _orderTransactionRepository.ExecuteTransactionAsync(async () =>
            {
                transactions.ForEach(p =>
                {
                    p.Status = EnumOrderTransactionStatus.Received;
                });
                _orderTransactionRepository.UpdateList(transactions);
                await _orderTransactionRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });
            return methodResult;
        }
    }
}
