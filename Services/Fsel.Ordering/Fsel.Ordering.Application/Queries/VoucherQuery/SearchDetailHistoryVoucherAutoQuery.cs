// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.VoucherQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
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

    public class SearchDetailHistoryVoucherAutoQuery : SearchDetailHistoryVoucherAutoQueryModel, IRequest<MethodResult<PagingItemsModel<HistoryVoucherModel>>>
    {
    }

    public class SearchDetailHistoryVoucherAutoQueryHandler : IRequestHandler<SearchDetailHistoryVoucherAutoQuery, MethodResult<PagingItemsModel<HistoryVoucherModel>>>
    {
        private readonly IVoucherRepository _voucherRepository;
        private readonly IUserService _userService;
        private readonly IOrderRepository _orderRepository;

        public SearchDetailHistoryVoucherAutoQueryHandler(IVoucherRepository voucherRepository, IUserService userService, IOrderRepository orderRepository)
        {
            _voucherRepository = voucherRepository;
            _userService = userService;
            _orderRepository = orderRepository;
        }

        public async Task<MethodResult<PagingItemsModel<HistoryVoucherModel>>> Handle(SearchDetailHistoryVoucherAutoQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<HistoryVoucherModel>>();

            if (string.IsNullOrEmpty(request.CodePrefix))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required));
                return methodResult;
            }

            var vouchers = from v in _voucherRepository.Queryable
                           join o in _orderRepository.Queryable.Where(p => p.Status == EnumOrderStatus.Payment || p.Status == EnumOrderStatus.New)
                               on v.Id equals o.VoucherId into orderGroup
                           from o in orderGroup.DefaultIfEmpty()
                           where v.CodePrefix == request.CodePrefix

                           select new HistoryVoucherModel
                           {
                               VoucherCode = v.Code,
                               VoucherId = v.Id,
                               Status = o != null,
                               UsageStatus = o != null ? "Đã đổi" : "Chưa đổi",
                               OrderCode = o.Code,
                               DayUsed = o.CreatedDate,
                               UserId = o.UserId,
                               Email = o.Email,
                           };

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                if (request.Keyword.IsValidEmail())
                {
                    var studentResult = await _userService.GetStudentByEmail(request.Keyword);
                    var student = studentResult.Content?.Result;
                    var userId = student?.Human?.UserId;
                    if (userId.HasValue)
                    {
                        vouchers = vouchers.Where(p => p.UserId == userId);
                    }
                    else
                    {
                        vouchers = vouchers.Where(p => p.Email == request.Keyword);
                    }
                }
                else
                {
                    vouchers = vouchers.Where(m => (!string.IsNullOrEmpty(m.OrderCode) && m.OrderCode.Contains(request.Keyword)) || (!string.IsNullOrEmpty(m.VoucherCode) && m.VoucherCode.Contains(request.Keyword)));
                }
            }

            if (request.Day.HasValue)
            {
                vouchers = vouchers.Where(p => p.DayUsed.HasValue && p.DayUsed.Value.Date == request.Day.Value.Date);
            }

            if (request.Status.HasValue)
            {
                vouchers = vouchers.Where(p => p.Status == request.Status);
            }

            int totalItem = await vouchers.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await vouchers
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            var userIds = lists.Where(p => p.UserId.HasValue).Select(p => p.UserId ?? default).Distinct().ToList();
            var studentResults = await _userService.GetStudentsByIdsAsync(userIds);
            var students = studentResults.Content?.Result;

            lists.ForEach(p =>
            {
                p.StudentCode = students?.FirstOrDefault(x => x.Human?.UserId == p.UserId)?.Human?.Code;
                p.Email = students?.FirstOrDefault(x => x.Human?.UserId == p.UserId)?.Human?.Email;
                p.FullName = students?.FirstOrDefault(x => x.Human?.UserId == p.UserId)?.Human?.FullName;
                p.PhoneNumber = students?.FirstOrDefault(x => x.Human?.UserId == p.UserId)?.Human?.PhoneNumber;
            });

            methodResult.Result = new PagingItemsModel<HistoryVoucherModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
