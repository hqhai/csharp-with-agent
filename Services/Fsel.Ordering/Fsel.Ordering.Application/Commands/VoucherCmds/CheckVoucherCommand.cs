// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.VoucherCmds
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Ordering.Application.Services.UserService;
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
        public Guid EventId { get; set; }
        public Guid? UserId { get; set; }
    }

    public class CheckVoucherCommandHandler : IRequestHandler<CheckVoucherCommand, MethodResult<CheckVoucherModel>>
    {
        private readonly IVoucherRepository _voucherRepository;
        private readonly AuthContext _authContext;
        private readonly IOrderRepository _orderRepository;
        private readonly IUserVoucherLockRepository _userVoucherLockRepository;
        private readonly IUserService _userService;
        private readonly IEventRepository _eventRepository;

        public CheckVoucherCommandHandler(IVoucherRepository voucherRepository,
            AuthContext authContext,
            IOrderRepository orderRepository,
            IUserVoucherLockRepository userVoucherLockRepository,
            IUserService userService,
            IEventRepository eventRepository)
        {
            _voucherRepository = voucherRepository;
            _authContext = authContext;
            _orderRepository = orderRepository;
            _userVoucherLockRepository = userVoucherLockRepository;
            _userService = userService;
            _eventRepository = eventRepository;
        }

        public async Task<MethodResult<CheckVoucherModel>> Handle(CheckVoucherCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<CheckVoucherModel>();
            var userId = request.UserId ?? _authContext.CurrentUserId;

            if (string.IsNullOrEmpty(request.Code))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required));
                return methodResult;
            }

            var voucher = await _voucherRepository.Queryable.Include(p => p.Orders).Include(p => p.VoucherPackages).FirstOrDefaultAsync(p => p.Code.ToLower() == request.Code.ToLower(), cancellationToken);
            if (voucher == null)
            {
                await ManageUserVoucherLockAsync(userId, cancellationToken);
                var result = await GetUserVoucherLockAsync(userId, cancellationToken);
                methodResult.Result = new CheckVoucherModel()
                {
                    VoucherId = null,
                    DiscountPrice = null,
                    Value = null,
                    TotalPrice = null,
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

            if (!Shared.Helpers.DateTimeHelper.IsCurrentDateInRange(voucher.StartDate, voucher.EndDate))
            {
                methodResult.AddErrorBadRequest(nameof(EnumVoucherErrorCode.VoucherHasExpired));
                return methodResult;
            }

            if (voucher.Orders.Where(p => p.Status == EnumOrderStatus.New || p.Status == EnumOrderStatus.Payment).Count() >= voucher.Quantity)
            {
                methodResult.AddErrorBadRequest(nameof(EnumVoucherErrorCode.VoucherOutOfQuantity));
                return methodResult;
            }

            if (voucher.Source == EnumVoucherSource.Retail && voucher.SourceUserId.HasValue && voucher.SourceUserId.Value == userId)
            {
                methodResult.AddErrorBadRequest(nameof(EnumVoucherErrorCode.NotSubjectToUse));
                return methodResult;
            }

            if (voucher.ApplicableSubjects == null || voucher.ApplicableSubjects.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumVoucherErrorCode.NotSubjectToUse));
                return methodResult;
            }

            var @event = await _eventRepository.Queryable.Include(pe => pe.PackageEvents).FirstOrDefaultAsync(e => e.Id == request.EventId, cancellationToken);
            if (@event == null || !@event.PackageEvents.Any(p => p.PackageId == request.PackageId && p.Status == EnumEventPackageStatus.Active) || voucher.EventIds == null || !voucher.EventIds.Contains(request.EventId))
            {
                methodResult.AddErrorBadRequest(nameof(EnumVoucherErrorCode.VoucherDoesNotApplyToThisPackage));
                return methodResult;
            }

            if (voucher.Source == EnumVoucherSource.Auto)
            {
                var vouchersAuto = await _voucherRepository.Queryable.Where(p => p.CodePrefix == voucher.CodePrefix).ToListAsync(cancellationToken);

                var voucherIds = vouchersAuto.Select(p => p.Id).ToList();

                var orders = await _orderRepository.Queryable.Where(p => p.UserId == userId && p.VoucherId.HasValue && voucherIds.Contains(p.VoucherId.Value) && p.Status == EnumOrderStatus.Payment).ToListAsync(cancellationToken);

                if (orders.Count >= voucher.NumberOfChanges)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumVoucherErrorCode.TheNumberOfUsesHasExpired));
                    return methodResult;
                }
            }

            var studentResult = await _userService.GetStudentByUserIdAsync(userId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddError(studentResult.Error);
                return methodResult;
            }
            var student = studentResult.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var currentDate = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);
            bool check = voucher.ApplicableSubjects.Any(applicableSubject =>
            {
                return applicableSubject switch
                {
                    EnumApplicableSubjectsVoucher.NewSale =>
                        !_orderRepository.Queryable.Any(p =>
                            p.UserId == userId &&
                            p.Status == EnumOrderStatus.Payment &&
                            !p.IsTrial),

                    EnumApplicableSubjectsVoucher.CurrentStudent =>
                        _orderRepository.Queryable.Any(p =>
                            p.UserId == userId &&
                            p.Status == EnumOrderStatus.Payment &&
                            !p.IsTrial) &&
                        student.ExpiredDate.HasValue &&
                        student.ExpiredDate > currentDate,

                    EnumApplicableSubjectsVoucher.Alumni =>
                        _orderRepository.Queryable.Any(p =>
                            p.UserId == userId &&
                            p.Status == EnumOrderStatus.Payment &&
                            !p.IsTrial) &&
                        (!student.ExpiredDate.HasValue ||
                         student.ExpiredDate < currentDate),

                    EnumApplicableSubjectsVoucher.Other =>
                        voucher.ApplicableEmails != null &&
                        !string.IsNullOrEmpty(student.Human?.Email) &&
                        voucher.ApplicableEmails.Contains(student.Human.Email),

                    _ => false
                };
            });

            if (!check)
            {
                methodResult.AddErrorBadRequest(nameof(EnumVoucherErrorCode.NotSubjectToUse));
                return methodResult;
            }

            var package = @event.PackageEvents.FirstOrDefault(pe => pe.PackageId == request.PackageId);
            if (package == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            decimal? discountPrice = null;
            decimal? totalPrice = null;
            if (voucher.Category == EnumVoucherCategory.Percent)
            {
                discountPrice = (decimal)NumberHelper.ConvertDoublePercent(Convert.ToDouble(package.Price * voucher.Value));
                totalPrice = package.Price - discountPrice;
            }
            else if (voucher.Category == EnumVoucherCategory.Money)
            {
                discountPrice = voucher.Value;
                totalPrice = discountPrice < package.Price ? package.Price - discountPrice : 0;
            }
            else if (voucher.Category == EnumVoucherCategory.Month)
            {
                discountPrice = 0;
                totalPrice = package.Price;
            }

            await ResetUserVoucherLockAsync(userId, cancellationToken);

            methodResult.Result = new CheckVoucherModel()
            {
                VoucherId = voucher.Id,
                Category = voucher.Category,
                DiscountPrice = discountPrice,
                Value = voucher.Value,
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

                var currentDate = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);

                if (userVoucherLock.Count == 10)
                {
                    userVoucherLock.ExpiredDate = currentDate.AddMinutes(15);
                }
                else if (userVoucherLock.Count == 20)
                {
                    userVoucherLock.ExpiredDate = currentDate.AddHours(2);
                }
                else if (userVoucherLock.Count == 50)
                {
                    userVoucherLock.ExpiredDate = currentDate.AddHours(6);
                }
                else if (userVoucherLock.Count == 100)
                {
                    userVoucherLock.ExpiredDate = currentDate.AddHours(24);
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
