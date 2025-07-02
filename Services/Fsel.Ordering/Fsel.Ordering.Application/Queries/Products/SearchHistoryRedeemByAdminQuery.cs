// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.Products
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Ordering.Application.Services.UserService;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Ordering.Domain.Models.QueryModels.Products;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchHistoryRedeemByAdminQuery : SearchHistoryRedeemByAdminQueryModel, IRequest<MethodResult<PagingItemsModel<SearchHistoryRedeemByAdminModel>>>
    {
    }

    public class SearchHistoryRedeemByAdminQueryHandler : IRequestHandler<SearchHistoryRedeemByAdminQuery, MethodResult<PagingItemsModel<SearchHistoryRedeemByAdminModel>>>
    {
        private readonly IUserService _userService;
        private readonly IOrderTransactionRepository _orderTransactionRepository;
        private readonly IMapper _mapper;

        public SearchHistoryRedeemByAdminQueryHandler(IUserService userService, IOrderTransactionRepository orderTransactionRepository, IMapper mapper)
        {
            _userService = userService;
            _orderTransactionRepository = orderTransactionRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<PagingItemsModel<SearchHistoryRedeemByAdminModel>>> Handle(SearchHistoryRedeemByAdminQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<SearchHistoryRedeemByAdminModel>>();

            var query = _orderTransactionRepository.Queryable.Include(p => p.Product).Where(p => p.Product != null && p.Type == EnumOrderTransactionType.Product && (p.Status == EnumOrderTransactionStatus.Requested || p.Status == EnumOrderTransactionStatus.Received)).Select(p => new SearchHistoryRedeemByAdminModel()
            {
                Id = p.Id,
                ProductId = p.Product!.Id,
                Code = p.Code,
                ProductCode = p.Product.Code,
                Status = p.Status,
                CreatedDate = p.CreatedDate,
                UpdatedDate = p.UpdatedDate,
                CreatedUserId = p.CreatedUserId
            });

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                var createdUserIds = query.Select(p => p.CreatedUserId).Distinct().ToList();
                var studentResults = await _userService.GetStudentsByIdsAsync(createdUserIds);
                var students = studentResults.Content?.Result;
                students = students?.Where(p => p.User != null && (!string.IsNullOrEmpty(p.User.FullName) && p.User.FullName.Contains(request.Keyword, StringComparison.InvariantCultureIgnoreCase) || (!string.IsNullOrEmpty(p.User.Email) && p.User.Email.Contains(request.Keyword, StringComparison.InvariantCultureIgnoreCase)))).ToList();

                var userIds = students?.Select(p => p!.UserId).ToList();

                query = query.Where(p => (!string.IsNullOrEmpty(p.Code) && p.Code.Contains(request.Keyword) || (!string.IsNullOrEmpty(p.ProductCode) && p.ProductCode.Contains(request.Keyword)) || (userIds != null && userIds.Contains(p.CreatedUserId))));
            }

            int totalItem = await query.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await query
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            var ids = lists.Select(p => p.CreatedUserId).Distinct().ToList();
            var results = await _userService.GetStudentsByIdsAsync(ids);
            var result = results.Content?.Result;

            lists.ForEach(p =>
            {
                var item = result?.FirstOrDefault(x => x.User != null && x.UserId == p.CreatedUserId);
                p.StudentCode = item?.User?.Code;
                p.StudentName = item?.User?.FullName;
                p.Email = item?.User?.Email;
                p.School = item?.School;
                p.CreatedDate = p.CreatedDate.HasValue ? p.CreatedDate.Value.ConvertTimeFromUtc(EnumCountryKey.Vietnam) : null;
                p.UpdatedDate = p.UpdatedDate.HasValue ? p.UpdatedDate.Value.ConvertTimeFromUtc(EnumCountryKey.Vietnam) : null;
            });

            methodResult.Result = new PagingItemsModel<SearchHistoryRedeemByAdminModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
