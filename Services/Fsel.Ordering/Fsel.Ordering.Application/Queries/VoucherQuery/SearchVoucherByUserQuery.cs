// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.VoucherQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Ordering.Application.Services.UserService;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Ordering.Domain.Models.QueryModels.Vouchers;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchVoucherByUserQuery : SearchVoucherByUserQueryModel, IRequest<MethodResult<PagingItemsModel<VoucherModel>>>
    {
    }

    public class SearchVoucherByUserQueryHandler : IRequestHandler<SearchVoucherByUserQuery, MethodResult<PagingItemsModel<VoucherModel>>>
    {
        private readonly IVoucherRepository _voucherRepository;
        private readonly IMapper _mapper;
        private readonly AuthContext _authContext;
        private readonly IOrderRepository _orderRepository;
        private readonly IUserService _userService;
        private const string StillValid = "Còn hiệu lực";
        private const string Expire = "Hết hiệu lực";
        private const string Still = "Đang còn";
        private const string OutOfStock = "Đã hết";

        public SearchVoucherByUserQueryHandler(IVoucherRepository voucherRepository, IMapper mapper, AuthContext authContext, IOrderRepository orderRepository, IUserService userService)
        {
            _voucherRepository = voucherRepository;
            _mapper = mapper;
            _authContext = authContext;
            _orderRepository = orderRepository;
            _userService = userService;
        }

        public async Task<MethodResult<PagingItemsModel<VoucherModel>>> Handle(SearchVoucherByUserQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<VoucherModel>>();

            var currentDate = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);

            var voucherQuery = await _voucherRepository.Queryable.Include(p => p.VoucherPackages).Where(p => p.Source == EnumVoucherSource.Admin && p.IsShowMyVoucher).Include(x => x.Orders).Where(p => p.StartDate <= currentDate && p.EndDate >= currentDate).OrderByDescending(x => x.CreatedDate).ToListAsync(cancellationToken);

            var voucherModels = _mapper.Map<IList<VoucherModel>>(voucherQuery);

            voucherModels.ForEach(x =>
            {
                var voucherModel = voucherQuery.First(p => p.Id == x.Id);
                var quantityUsed = voucherModel.Orders.Where(p => p.Status == EnumOrderStatus.New || p.Status == EnumOrderStatus.Payment).Count();
                x.PackageIds = voucherModel.VoucherPackages.Select(p => p.PackageId).ToList();
                x.QuantityUsed = quantityUsed;
                x.RemainingQuantity = x.Quantity - quantityUsed;
                x.Status = quantityUsed < x.Quantity;
                x.ItemStatus = quantityUsed < x.Quantity ? Still : OutOfStock;
                x.Duration = currentDate < x.EndDate ? StillValid : Expire;
            });

            voucherModels = voucherModels.Where(m => m.Status).ToList();

            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
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

            var removeVoucher = new List<VoucherModel>();

            var isHaveTuitionBill = _orderRepository.Queryable.Any(p => p.UserId == _authContext.CurrentUserId && p.Status == EnumOrderStatus.Payment && !p.IsTrial);

            foreach (var voucher in voucherModels)
            {
                if (voucher.ApplicableSubjects!.Any(p => p == EnumApplicableSubjectsVoucher.NewSale) && !isHaveTuitionBill)
                {
                    continue;
                }
                if (voucher.ApplicableSubjects!.Any(p => p == EnumApplicableSubjectsVoucher.CurrentStudent) && isHaveTuitionBill && student.ExpiredDate.HasValue &&
                        student.ExpiredDate > currentDate)
                {
                    continue;
                }
                if (voucher.ApplicableSubjects!.Any(p => p == EnumApplicableSubjectsVoucher.Alumni) && isHaveTuitionBill && student.ExpiredDate.HasValue &&
                        student.ExpiredDate < currentDate)
                {
                    continue;
                }
                if (voucher.ApplicableSubjects!.Any(p => p == EnumApplicableSubjectsVoucher.Other))
                {
                    if (voucher.ApplicableEmails != null &&
                            !string.IsNullOrEmpty(student.User?.Email) &&
                            voucher.ApplicableEmails.Contains(student.User.Email))
                    {
                        continue;
                    }
                }

                removeVoucher.Add(voucher);
            }

            removeVoucher.ForEach(x =>
            {
                voucherModels.Remove(x);
            });

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                voucherModels = voucherModels.Where(m => (m.Name ?? string.Empty).ToLower().Trim().Contains(request.Keyword.ToLower().Trim(), StringComparison.InvariantCultureIgnoreCase) || (m.Code ?? string.Empty).ToLower().Trim().Contains(request.Keyword.ToLower().Trim(), StringComparison.InvariantCultureIgnoreCase)).ToList();
            }

            if (!request.Sort)
            {
                voucherModels = voucherModels.OrderByDescending(p => p.CreatedDate).ThenBy(p => p.Name).ToList();
            }
            else
            {
                voucherModels = voucherModels.OrderBy(p => p.EndDate).ThenBy(p => p.Name).ToList();
            }

            int totalItem = voucherModels.Count;
            var lists = voucherModels.ApplyPaging(request).ToList();

            methodResult.Result = new PagingItemsModel<VoucherModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
