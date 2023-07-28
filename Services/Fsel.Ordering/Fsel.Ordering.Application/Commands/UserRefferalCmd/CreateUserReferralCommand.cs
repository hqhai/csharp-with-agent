// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.UserRefferalCmd
{
    using System;
    using System.Threading;
    using Fsel.Common.ActionResults;
    using Fsel.Ordering.Application.Services.SystemService;
    using Fsel.Ordering.Application.Services.SystemService.Models;
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Ordering.Domain.Enums;
    using Fsel.Ordering.Domain.Enums.ErrorCodes;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateUserReferralCommand : IRequest<MethodResult<bool>>
    {
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
    }

    public class CreateUserReferralCommandHandler : IRequestHandler<CreateUserReferralCommand, MethodResult<bool>>
    {
        private readonly IUserReferralRepository _userReferralRepository;
        private readonly ISystemService _systemService;
        private readonly IVoucherRepository _voucherRepository;
        private readonly IPackageRepository _packageRepository;

        public CreateUserReferralCommandHandler(IUserReferralRepository userReferralRepository
            , ISystemService systemService
            , IVoucherRepository voucherRepository
            , IPackageRepository packageRepository)
        {
            _userReferralRepository = userReferralRepository;
            _systemService = systemService;
            _voucherRepository = voucherRepository;
            _packageRepository = packageRepository;
        }

        public async Task<MethodResult<bool>> Handle(CreateUserReferralCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();
            var userReferral = await _userReferralRepository.Queryable.Where(x => x.SenderId == request.SenderId).OrderByDescending(item => item.IndexNumber).FirstOrDefaultAsync(cancellationToken);

            var userReferralReceivers = await _userReferralRepository.Queryable.Where(x => x.ReceiverId == request.ReceiverId).ToListAsync(cancellationToken);
            if (userReferralReceivers.Count > 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumUserReferralErrorCode.UsedReferralCode));
                return methodResult;
            }

            UserReferral userReferralCreate = new UserReferral();
            userReferralCreate.IndexNumber = (userReferral?.IndexNumber ?? default) + 1;
            userReferralCreate.SenderId = request.SenderId;
            userReferralCreate.ReceiverId = request.ReceiverId;
            if (!userReferralCreate.IsValid())
            {
                methodResult.AddErrorBadRequest(userReferralCreate.ErrorMessages);
                return methodResult;
            }

            var referralDiscountConfigResults = await _systemService.GetReferralDiscountConfigAsync();
            if (!referralDiscountConfigResults.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallSystemServiceError));
                return methodResult;
            }
            var referralDiscountConfigs = referralDiscountConfigResults?.Content?.Result;
            if (referralDiscountConfigs != null && referralDiscountConfigs.Count > 0)
            {
                await AddVoucher(userReferralCreate.IndexNumber, referralDiscountConfigs, request.SenderId, false);
                await AddVoucher(userReferralCreate.IndexNumber, referralDiscountConfigs, request.ReceiverId, true);
            }
            await _userReferralRepository.ExecuteTransactionAsync(async () =>
            {
                _userReferralRepository.Add(userReferralCreate);
                await _userReferralRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = true;
                return methodResult;
            });
            return methodResult;
        }

        public async Task AddVoucher(int index, IList<ReferralDiscountConfigModel>? referralDiscountConfigs, Guid userId, bool isReceiver)
        {
            ArgumentNullException.ThrowIfNull(referralDiscountConfigs);

            var customerTypes = Enum.GetValues(typeof(EnumCustomerType)).Cast<EnumCustomerType>().ToList();
            var courseLevels = Enum.GetValues(typeof(EnumCourseLevel)).Cast<EnumCourseLevel>().ToList();
            var referralDiscountConfig = referralDiscountConfigs.OrderBy(x => x.IndexNumber).FirstOrDefault(x => index >= x.IndexNumber);

            if (referralDiscountConfig == null)
            {
                return;
            }

            var voucher = new Voucher
            {
                Name = "ReferralCode",
                StartDate = DateTime.Now.Date,
                EndDate = DateTime.Now.Date.AddMonths(1),
                IsGlobal = false,
                IsActive = true,
                CustomerTypes = customerTypes,
                CourseLevels = courseLevels,
                VoucherPackages = await _packageRepository.Queryable.Select(x => new VoucherPackage
                {
                    Percentage = isReceiver ? (referralDiscountConfig.RecevierDiscountValue ?? default) : (referralDiscountConfig.SenderDiscountValue ?? default),
                    PackageId = x.Id
                }).ToListAsync(),
                UserVouchers = new List<UserVoucher> { new UserVoucher { UserId = userId, Status = EnumUserVoucherStatus.NotUsed } }
            };
            _voucherRepository.Add(voucher);
            await _voucherRepository.UnitOfWork.SaveEntitiesAsync().ConfigureAwait(false);
        }
    }
}
