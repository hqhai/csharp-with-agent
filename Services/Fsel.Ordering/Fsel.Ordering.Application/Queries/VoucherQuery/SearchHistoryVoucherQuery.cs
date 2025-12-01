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
        private readonly IOrderRepository _orderRepository;
        private readonly IUserService _userService;

        public SearchHistoryVoucherQueryHandler(IOrderRepository orderRepository, IUserService userService)
        {
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
                Status = p.Status == EnumOrderStatus.Payment,
                VoucherId = p.VoucherId ?? default,
                UserId = p.UserId,
                VoucherCode = p.Voucher == null ? null : p.Voucher.Code
            });

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                var studentResult = await _userService.GetStudentByEmail(request.Keyword);
                var student = studentResult.Content?.Result;
                var userId = student?.UserId;

                if (userId.HasValue)
                {
                    orders = orders.Where(p => p.UserId == userId);
                }
                else
                {
                    orders = orders.Where(m => (m.OrderCode ?? string.Empty).ToLower().Trim().Contains(request.Keyword.ToLower().Trim()) || (m.VoucherCode ?? string.Empty).ToLower().Trim().Contains(request.Keyword.ToLower().Trim()));
                }
            }

            if (request.Day.HasValue)
            {
                orders = orders.Where(p => p.DayUsed.HasValue && p.DayUsed.Value.Date == request.Day.Value.Date);
            }

            if (request.Status.HasValue)
            {
                orders = orders.Where(p => p.Status == request.Status);
            }

            orders = orders.OrderByDescending(m => m.DayUsed);

            int totalItem = await orders.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await orders
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            var nullableGuids = lists.Where(p => p.UserId.HasValue).Select(p => p.UserId).Distinct().ToList();
            var userIds = new List<Guid>();
            nullableGuids.ForEach(p =>
            {
                userIds.Add(p!.Value);
            });
            var studentResults = await _userService.GetStudentsByIdsAsync(userIds);
            var students = studentResults.Content?.Result;
            lists.ForEach(p =>
            {
                p.StudentCode = students?.FirstOrDefault(x => x?.UserId == p.UserId)?.User?.Code;
                p.Email = students?.FirstOrDefault(x => x?.UserId == p.UserId)?.User?.Email;
            });
            methodResult.Result = new PagingItemsModel<HistoryVoucherModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
