// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.VoucherCmds
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Ordering.Domain.Enums.ErrorCodes;
    using Fsel.Ordering.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class DeleteVouchersCommand : IRequest<MethodResult<bool>>
    {
        public IList<Guid>? VoucherIds { get; set; }
    }

    public class DeleteVouchersCommandHandler : IRequestHandler<DeleteVouchersCommand, MethodResult<bool>>
    {
        private readonly IVoucherRepository _voucherRepository;

        public DeleteVouchersCommandHandler(IVoucherRepository voucherRepository)
        {
            _voucherRepository = voucherRepository;
        }

        public async Task<MethodResult<bool>> Handle(DeleteVouchersCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            if (request.VoucherIds == null || request.VoucherIds.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required));
                return methodResult;
            }

            var vouchers = await _voucherRepository.Queryable.Include(p => p.Orders).Where(p => request.VoucherIds.Contains(p.Id)).ToListAsync(cancellationToken);
            if (vouchers == null || vouchers.Count != request.VoucherIds.Count)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }
            if (vouchers.Any(p => p.Orders.Any()))
            {
                methodResult.AddErrorBadRequest(nameof(EnumVoucherErrorCode.VoucherIsUsed));
                return methodResult;
            }

            await _voucherRepository.ExecuteTransactionAsync(async () =>
            {
                await _voucherRepository.DeleteListAsync(vouchers);
                await _voucherRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }
    }
}
