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
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Ordering.Domain.Enums.ErrorCodes;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Ordering.Infrastructure.Repositories;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchVoucherByUserQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<VoucherModel>>>
    {
    }

    public class SearchVoucherByUserQueryHandler : IRequestHandler<SearchVoucherByUserQuery, MethodResult<PagingItemsModel<VoucherModel>>>
    {
        private readonly IVoucherRepository _voucherRepository;
        private readonly IMapper _mapper;
        private readonly AuthContext _authContext;
        private readonly IOrderRepository _orderRepository;
        private readonly IUserService _userService;

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

            var voucherQuery = await _voucherRepository.Queryable.Where(p => p.Source == EnumVoucherSource.Admin).Include(x => x.Orders).Where(p => p.StartDate <= currentDate && p.EndDate >= currentDate).OrderByDescending(x => x.CreatedDate).ToListAsync(cancellationToken);

            var voucherModels = _mapper.Map<IList<VoucherModel>>(voucherQuery);

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

            voucherModels.ForEach(x =>
            {
                var quantityUsed = voucherQuery.First(p => p.Id == x.Id).Orders.Where(p => p.Status == EnumOrderStatus.New || p.Status == EnumOrderStatus.Payment).Count();
                x.QuantityUsed = quantityUsed;
                x.RemainingQuantity = x.Quantity - quantityUsed;
                x.Status = quantityUsed < x.Quantity;
                x.ItemStatus = quantityUsed < x.Quantity ? "Đang còn" : "Đã hết";
                x.Duration = currentDate < x.EndDate ? "Còn hiệu lực" : "Hết hiệu lực";
            });

            var removeVoucher = new List<VoucherModel>();

            foreach (var voucher in voucherModels)
            {
                var isOrderPayment = _orderRepository.Queryable.Any(p => p.UserId == _authContext.CurrentUserId && p.Status == EnumOrderStatus.Payment && !p.IsTrial);

                if (voucher.ApplicableSubjects!.Any(p => p == EnumApplicableSubjectsVoucher.NewSale) && !isOrderPayment)
                {
                    continue;
                }
                if (voucher.ApplicableSubjects!.Any(p => p == EnumApplicableSubjectsVoucher.CurrentStudent) && isOrderPayment && student.ExpiredDate.HasValue &&
                        student.ExpiredDate > currentDate)
                {
                    continue;
                }
                if (voucher.ApplicableSubjects!.Any(p => p == EnumApplicableSubjectsVoucher.Alumni) && isOrderPayment && student.ExpiredDate.HasValue &&
                        student.ExpiredDate < currentDate)
                {
                    continue;
                }
                if (voucher.ApplicableSubjects!.Any(p => p == EnumApplicableSubjectsVoucher.Other))
                {
                    if (voucher.ApplicableEmails != null &&
                            !string.IsNullOrEmpty(student.Human?.Email) &&
                            voucher.ApplicableEmails.Contains(student.Human.Email))
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

            voucherModels = voucherModels.Where(m => m.Status).ToList();

            int totalItem = voucherModels.Count;
            var lists = voucherModels.ApplySortAndPaging(request).ToList();

            methodResult.Result = new PagingItemsModel<VoucherModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
