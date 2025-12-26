// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.VoucherQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
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
        private readonly IVoucherRepository _voucherRepository;

        public SearchHistoryVoucherQueryHandler(IOrderRepository orderRepository, IUserService userService, IVoucherRepository voucherRepository)
        {
            _orderRepository = orderRepository;
            _userService = userService;
            _voucherRepository = voucherRepository;
        }

        public async Task<MethodResult<PagingItemsModel<HistoryVoucherModel>>> Handle(SearchHistoryVoucherQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<HistoryVoucherModel>>();

            var orders = from v in _voucherRepository.Queryable
                         join o in _orderRepository.Queryable.Where(p => p.Status == EnumOrderStatus.Payment || p.Status == EnumOrderStatus.New)
                         on v.Id equals o.VoucherId
                         select new HistoryVoucherModel
                         {
                             OrderCode = o.Code,
                             DayUsed = o.CreatedDate,
                             Status = o.Status == EnumOrderStatus.Payment,
                             VoucherId = v.Id,
                             UserId = o.UserId,
                             VoucherCode = v.Code
                         };

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                if (request.Keyword.IsValidEmail())
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
                        orders = orders.Where(p => p.Email == request.Keyword);
                    }
                }
                else
                {
                    orders = orders.Where(m => (!string.IsNullOrEmpty(m.OrderCode) && m.OrderCode.Contains(request.Keyword)) || (!string.IsNullOrEmpty(m.VoucherCode) && m.VoucherCode.Contains(request.Keyword)));
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

            var userIds = lists.Where(p => p.UserId.HasValue).Select(p => p.UserId ?? default).Distinct().ToList();
            var studentResults = await _userService.GetStudentsByIdsAsync(userIds);
            var students = studentResults.Content?.Result;

            lists.ForEach(p =>
            {
                p.StudentCode = students?.FirstOrDefault(x => x.UserId == p.UserId)?.User?.Code;
                p.Email = students?.FirstOrDefault(x => x.UserId == p.UserId)?.User?.Email;
                p.FullName = students?.FirstOrDefault(x => x.UserId == p.UserId)?.User?.FullName;
                p.PhoneNumber = students?.FirstOrDefault(x => x.UserId == p.UserId)?.User?.PhoneNumber;
            });

            methodResult.Result = new PagingItemsModel<HistoryVoucherModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
