// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.VoucherQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
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

        public SearchDetailHistoryVoucherAutoQueryHandler(IVoucherRepository voucherRepository, IUserService userService)
        {
            _voucherRepository = voucherRepository;
            _userService = userService;
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

            var vouchers = _voucherRepository.Queryable.Where(p => p.CodePrefix.ToLower() == request.CodePrefix.ToLower()).Include(p => p.Orders).Select(p => new HistoryVoucherModel()
            {
                VoucherCode = p.Code,
                Status = p.Orders.Where(x => x.Status == EnumOrderStatus.New || x.Status == EnumOrderStatus.Payment).Any(),
                UsageStatus = p.Orders.Where(x => x.Status == EnumOrderStatus.New || x.Status == EnumOrderStatus.Payment).Any() ? "Đã đổi" : "Chưa đổi",
                OrderCode = !p.Orders.Any() ? null : p.Orders.First().Code,
                DayUsed = !p.Orders.Any() ? null : p.Orders.First().CreatedDate,
                UserId = !p.Orders.Any() ? null : p.Orders.First().UserId,
            });

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                var studentResult = await _userService.GetStudentByEmail(request.Keyword);
                var student = studentResult.Content?.Result;
                var userId = student?.UserId;
                if (userId.HasValue)
                {
                    vouchers = vouchers.Where(p => p.UserId == userId);
                }
                else
                {
                    vouchers = vouchers.Where(m => (m.OrderCode ?? string.Empty).ToLower().Trim().Contains(request.Keyword.ToLower().Trim()) || (m.VoucherCode ?? string.Empty).ToLower().Trim().Contains(request.Keyword.ToLower().Trim()));
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
