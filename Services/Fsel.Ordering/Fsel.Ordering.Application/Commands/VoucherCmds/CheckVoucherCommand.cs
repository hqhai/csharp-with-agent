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
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class CheckVoucherCommand : IRequest<MethodResult<CheckVoucherModel>>
    {
        public string? Code { get; set; }
        public Guid PackageId { get; set; }
    }

    public class CheckVoucherCommandHandler : IRequestHandler<CheckVoucherCommand, MethodResult<CheckVoucherModel>>
    {
        private readonly IVoucherRepository _voucherRepository;
        private readonly AuthContext _authContext;
        private readonly IOrderRepository _orderRepository;
        private readonly IPackageRepository _packageRepository;

        public CheckVoucherCommandHandler(IVoucherRepository voucherRepository, AuthContext authContext, IOrderRepository orderRepository, IPackageRepository packageRepository)
        {
            _voucherRepository = voucherRepository;
            _authContext = authContext;
            _orderRepository = orderRepository;
            _packageRepository = packageRepository;
        }

        public async Task<MethodResult<CheckVoucherModel>> Handle(CheckVoucherCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<CheckVoucherModel>();

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

            if (!voucher.VoucherPackages.Any(p => p.PackageId == request.PackageId))
            {
                methodResult.AddErrorBadRequest(nameof(EnumVoucherErrorCode.VoucherDoesNotApplyToThisPackage));
                return methodResult;
            }

            if (!DateTimeHelper.IsCurrentDateInRange(voucher.StartDate, voucher.EndDate))
            {
                methodResult.AddErrorBadRequest(nameof(EnumVoucherErrorCode.VoucherHasExpired));
                return methodResult;
            }

            if (voucher.Orders.Where(p => p.Status == EnumOrderStatus.New || p.Status == EnumOrderStatus.Payment).Count() >= voucher.Quantity)
            {
                methodResult.AddErrorBadRequest(nameof(EnumVoucherErrorCode.VoucherOutOfQuantity));
                return methodResult;
            }

            if (voucher.Source == EnumVoucherSource.Retail && voucher.SourceUserId.HasValue && voucher.SourceUserId.Value == _authContext.CurrentUserId)
            {
                methodResult.AddErrorBadRequest(nameof(EnumVoucherErrorCode.NotSubjectToUse));
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

            var package = await _packageRepository.GetByIdAsync(request.PackageId);
            if (package == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var discountPrice = (decimal)NumberHelper.ConvertDoublePercent(Convert.ToDouble(package.Price * voucher.Percent));
            var totalPrice = package.Price - discountPrice;

            methodResult.Result = new CheckVoucherModel()
            {
                VoucherId = voucher.Id,
                DiscountPrice = discountPrice,
                Percent = voucher.Percent,
                TotalPrice = totalPrice,
            };
            return methodResult;
        }
    }
}
