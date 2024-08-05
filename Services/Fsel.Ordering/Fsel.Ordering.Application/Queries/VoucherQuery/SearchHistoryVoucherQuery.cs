// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.VoucherQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
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

    public class SearchHistoryVoucherQuery : SearchHistoryVoucherQueryModel, IRequest<MethodResult<PagingItemsModel<HistoryVoucherModel>>>
    {
    }

    public class SearchHistoryVoucherQueryHandler : IRequestHandler<SearchHistoryVoucherQuery, MethodResult<PagingItemsModel<HistoryVoucherModel>>>
    {
        private readonly IVoucherRepository _voucherRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IUserService _userService;

        public SearchHistoryVoucherQueryHandler(IVoucherRepository voucherRepository, IOrderRepository orderRepository, IUserService userService)
        {
            _voucherRepository = voucherRepository;
            _orderRepository = orderRepository;
            _userService = userService;
        }

        public async Task<MethodResult<PagingItemsModel<HistoryVoucherModel>>> Handle(SearchHistoryVoucherQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<HistoryVoucherModel>>();

            var orders = _orderRepository.Queryable.Include(p => p.Voucher).Where(p => p.VoucherId.HasValue && (p.Status == EnumOrderStatus.New || p.Status == EnumOrderStatus.Payment)).Select(p => new HistoryVoucherModel
            {
                OrderCode = p.Code,
                DayUsed = p.CreatedDate,
                Status = p.Status,
                VoucherId = p.VoucherId ?? default,
                UserId = p.UserId,
                VoucherCode = p.Voucher == null ? null : p.Voucher.Code
            });

            if (request.Day.HasValue)
            {
                orders = orders.Where(p => p.DayUsed.Date == request.Day.Value.Date);
            }

            if (request.Status.HasValue)
            {
                orders = orders.Where(p => request.Status == true ? p.Status == EnumOrderStatus.Payment : p.Status == EnumOrderStatus.New);
            }
            int totalItem = await orders.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await orders
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            var userIds = lists.Select(p => p.UserId).Distinct().ToList();
            var studentResults = await _userService.GetStudentsByIdsAsync(userIds);
            var students = studentResults.Content?.Result;
            lists.ForEach(p =>
            {
                p.StudentCode = students?.FirstOrDefault(x => x.Human?.UserId == p.UserId)?.Human?.Code;
                p.Email = students?.FirstOrDefault(x => x.Human?.UserId == p.UserId)?.Human?.Email;
            });
            methodResult.Result = new PagingItemsModel<HistoryVoucherModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
