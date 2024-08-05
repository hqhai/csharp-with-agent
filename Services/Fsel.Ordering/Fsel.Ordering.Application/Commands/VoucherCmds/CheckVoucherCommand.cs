// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.VoucherCmds
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Ordering.Domain.Enums.ErrorCodes;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class CheckVoucherCommand : IRequest<MethodResult<Guid>>
    {
        public string? Code { get; set; }
        public Guid PackageId { get; set; }
    }

    public class CheckVoucherCommandHandler : IRequestHandler<CheckVoucherCommand, MethodResult<Guid>>
    {
        private readonly IVoucherRepository _voucherRepository;
        private readonly AuthContext _authContext;
        private readonly IOrderRepository _orderRepository;

        public CheckVoucherCommandHandler(IVoucherRepository voucherRepository, AuthContext authContext, IOrderRepository orderRepository)
        {
            _voucherRepository = voucherRepository;
            _authContext = authContext;
            _orderRepository = orderRepository;
        }

        public async Task<MethodResult<Guid>> Handle(CheckVoucherCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<Guid>();

            if (string.IsNullOrEmpty(request.Code))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required));
                return methodResult;
            }

            var voucher = await _voucherRepository.Queryable.Include(p => p.Orders).Include(p => p.VoucherPackages).FirstOrDefaultAsync(p => p.Code.ToLower() == request.Code.ToLower(), cancellationToken);
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

            if (!voucher.VoucherPackages.Any(p => p.PackageId == request.PackageId))
            {
                methodResult.AddErrorBadRequest(nameof(EnumVoucherErrorCode.VoucherDoesNotApplyToThisPackage));
                return methodResult;
            }

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

            if (voucher.Orders.Where(p => p.Status == EnumOrderStatus.New || p.Status == EnumOrderStatus.Payment).Count() >= voucher.Quantity)
            {
                methodResult.AddErrorBadRequest(nameof(EnumVoucherErrorCode.VoucherOutOfQuantity));
                return methodResult;
            }

            if (voucher.VoucherType == EnumVoucherType.NewSale)
            {
                if (await _orderRepository.Queryable.AnyAsync(p => p.Status == EnumOrderStatus.Payment && p.UserId == _authContext.CurrentUserId && !p.IsTrial, cancellationToken))
                {
                    methodResult.AddErrorBadRequest(nameof(EnumVoucherErrorCode.NotSubjectToUse));
                    return methodResult;
                }
            }

            methodResult.Result = voucher.Id;
            return methodResult;
        }
    }
}
