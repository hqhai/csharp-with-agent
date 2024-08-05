// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.VoucherCmds
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Ordering.Domain.Enums.ErrorCodes;
    using Fsel.Ordering.Domain.IRepositories;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class CheckVoucherCommand : IRequest<MethodResult<bool>>
    {
        public string? Code { get; set; }
    }

    public class CheckVoucherCommandHandler : IRequestHandler<CheckVoucherCommand, MethodResult<bool>>
    {
        private readonly IVoucherRepository _voucherRepository;

        public CheckVoucherCommandHandler(IVoucherRepository voucherRepository)
        {
            _voucherRepository = voucherRepository;
        }

        public async Task<MethodResult<bool>> Handle(CheckVoucherCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            if (string.IsNullOrEmpty(request.Code))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required));
                return methodResult;
            }

            var voucher = await _voucherRepository.Queryable.Include(p => p.Orders).FirstOrDefaultAsync(p => p.Code.ToLower() == request.Code.ToLower(), cancellationToken);
            if (voucher == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumVoucherErrorCode.VoucherNotExist));
                return methodResult;
            }
            if (!voucher.IsActive)
            {
                methodResult.AddErrorBadRequest(nameof(EnumVoucherErrorCode.VoucherNotActive));
                return methodResult;
            }

            var currentDate = DateTime.UtcNow;

            if (voucher.EndDate.HasValue && (voucher.StartDate.Date > currentDate.Date && voucher.EndDate.Value.Date < currentDate.Date))
            {
                methodResult.AddErrorBadRequest(nameof(EnumVoucherErrorCode.VoucherHasExpired));
                return methodResult;
            }
            else if (!voucher.EndDate.HasValue && voucher.StartDate.Date > currentDate.Date)
            {
                methodResult.AddErrorBadRequest(nameof(EnumVoucherErrorCode.VoucherHasExpired));
                return methodResult;
            }

            if (voucher.Orders.Count >= voucher.Quantity)
            {
                methodResult.AddErrorBadRequest(nameof(EnumVoucherErrorCode.VoucherOutOfQuantity));
                return methodResult;
            }

            methodResult.Result = true;
            return methodResult;
        }
    }
}
