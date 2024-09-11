// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.VoucherCmds
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Ordering.Domain.Entities;
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
        private readonly IUserVoucherLockRepository _userVoucherLockRepository;

        public CheckVoucherCommandHandler(IVoucherRepository voucherRepository, AuthContext authContext, IOrderRepository orderRepository, IPackageRepository packageRepository, IUserVoucherLockRepository userVoucherLockRepository)
        {
            _voucherRepository = voucherRepository;
            _authContext = authContext;
            _orderRepository = orderRepository;
            _packageRepository = packageRepository;
            _userVoucherLockRepository = userVoucherLockRepository;
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
                await ManageUserVoucherLockAsync(_authContext.CurrentUserId, cancellationToken);
                var result = await GetUserVoucherLockAsync(_authContext.CurrentUserId, cancellationToken);
                methodResult.Result = new CheckVoucherModel()
                {
                    VoucherId = default,
                    DiscountPrice = 0,
                    Percent = 0,
                    TotalPrice = 0,
                    UserVoucherLock = result
                };
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

            await ResetUserVoucherLockAsync(_authContext.CurrentUserId, cancellationToken);

            methodResult.Result = new CheckVoucherModel()
            {
                VoucherId = voucher.Id,
                DiscountPrice = discountPrice,
                Percent = voucher.Percent,
                TotalPrice = totalPrice,
            };

            return methodResult;
        }

        private async Task ManageUserVoucherLockAsync(Guid userId, CancellationToken cancellationToken)
        {
            var userVoucherLock = await _userVoucherLockRepository.Queryable.FirstOrDefaultAsync(p => p.CreatedUserId == userId, cancellationToken);
            if (userVoucherLock == null)
            {
                _userVoucherLockRepository.Add(new UserVoucherLock()
                {
                    Count = 1,
                });
            }
            else
            {
                userVoucherLock.Count += 1;

                if (userVoucherLock.Count == 10)
                {
                    userVoucherLock.ExpiredDate = DateTime.UtcNow.AddMinutes(15);
                }
                else if (userVoucherLock.Count == 20)
                {
                    userVoucherLock.ExpiredDate = DateTime.UtcNow.AddHours(2);
                }
                else if (userVoucherLock.Count == 50)
                {
                    userVoucherLock.ExpiredDate = DateTime.UtcNow.AddHours(6);
                }
                else if (userVoucherLock.Count == 100)
                {
                    userVoucherLock.ExpiredDate = DateTime.UtcNow.AddHours(24);
                }
                else if (userVoucherLock.Count == 200)
                {
                    userVoucherLock.ExpiredDate = DateTime.MaxValue;
                    userVoucherLock.IsLockForever = true;
                }

                _userVoucherLockRepository.Update(userVoucherLock);
            }
            await _userVoucherLockRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        private async Task<UserVoucherLock?> GetUserVoucherLockAsync(Guid userId, CancellationToken cancellationToken)
        {
            var userVoucherLock = await _userVoucherLockRepository.Queryable.FirstOrDefaultAsync(p => p.CreatedUserId == userId, cancellationToken);
            return userVoucherLock;
        }

        private async Task ResetUserVoucherLockAsync(Guid userId, CancellationToken cancellationToken)
        {
            var userVoucherLock = await _userVoucherLockRepository.Queryable.FirstOrDefaultAsync(p => p.CreatedUserId == userId, cancellationToken);
            if (userVoucherLock != null)
            {
                userVoucherLock.Count = 0;
                userVoucherLock.ExpiredDate = null;
                userVoucherLock.IsLockForever = false;
                _userVoucherLockRepository.Update(userVoucherLock);
                await _userVoucherLockRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
