// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.OrderQuery.V1i2
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Ordering.Application.Services.UserService;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.EntityModels.V1i2;
    using Fsel.Ordering.Domain.Models.QueryModels.Oders.V1i2;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchOrderQuery : SearchOrderQueryModel, IRequest<MethodResult<PagingItemsModel<SearchOrderModel>>>
    {
    }

    public class SearchOrderQueryHandler : IRequestHandler<SearchOrderQuery, MethodResult<PagingItemsModel<SearchOrderModel>>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;

        public SearchOrderQueryHandler(IOrderRepository orderRepository, AuthContext authContext, IUserService userService)
        {
            _orderRepository = orderRepository;
            _authContext = authContext;
            _userService = userService;
        }

        public async Task<MethodResult<PagingItemsModel<SearchOrderModel>>> Handle(SearchOrderQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<SearchOrderModel>>();

            var query = _orderRepository.Queryable.Include(p => p.Package).Where(p => !p.IsTrial).Select(x => new SearchOrderModel
            {
                Id = x.Id,
                Code = x.Code,
                UserId = x.UserId,
                CreatedDate = x.UpdatedDate ?? x.CreatedDate,
                UpdatedDate = x.UpdatedDate,
                CreatedFullName = x.CreatedFullName,
                Status = x.Status,
                PaymentMethod = x.PaymentMethod,
                PackageId = x.PackageId,
                MonthNumber = x.Package == null ? null : x.Package.MonthNumber,
                RevenueType = x.RevenueType,
                TotalPrice = x.TotalPrice,
            }).ToList();



            if (request.IsNew.HasValue && request.IsNew == true)
            {
                query = query.Where(p => p.Status == EnumOrderStatus.New && (p.PaymentMethod == EnumPaymentMethodStatus.BankTransfer || p.PaymentMethod == EnumPaymentMethodStatus.Card)).ToList();
            }
            else if (request.IsNew.HasValue && request.IsNew == false)
            {
                query = query.Where(p => p.Status != EnumOrderStatus.New || (p.Status == EnumOrderStatus.New && (p.PaymentMethod == EnumPaymentMethodStatus.Payoo || p.PaymentMethod == EnumPaymentMethodStatus.AppStore || p.PaymentMethod == EnumPaymentMethodStatus.CHPlay))).ToList();
            }

            if (_authContext.Roles?.FirstOrDefault() == EnumRole.Student.ToString())
            {
                query = query.Where(p => p.UserId == _authContext.CurrentUserId).ToList();
            }

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                if (request.Keyword.IsValidEmail())
                {
                    query = query.Where(p => !string.IsNullOrEmpty(p.Email) && p.Email.Contains(request.Keyword, StringComparison.InvariantCultureIgnoreCase)).ToList();
                }
                else
                {
                    query = query.Where(p => (!string.IsNullOrEmpty(p.Code) && p.Code.Contains(request.Keyword, StringComparison.InvariantCultureIgnoreCase)) || (!string.IsNullOrEmpty(p.FullName) && p.FullName.Contains(request.Keyword, StringComparison.InvariantCultureIgnoreCase))).ToList();
                }
            }

            if (request.PackageIds != null && request.PackageIds.Count > 0)
            {
                query = query.Where(p => p.PackageId.HasValue && request.PackageIds.Contains(p.PackageId.Value)).ToList();
            }

            if (request.StartDate.HasValue && request.EndDate.HasValue)
            {
                query = query.Where(p => p.CreatedDate.HasValue && request.StartDate.Value.Date <= p.CreatedDate.Value.Date && request.EndDate.Value.Date >= p.CreatedDate.Value.Date).ToList();
            }
            else if (request.StartDate.HasValue)
            {
                query = query.Where(p => p.CreatedDate.HasValue && request.StartDate.Value.Date <= p.CreatedDate.Value.Date).ToList();
            }
            else if (request.EndDate.HasValue)
            {
                query = query.Where(p => p.CreatedDate.HasValue && request.EndDate.Value.Date >= p.CreatedDate.Value.Date).ToList();
            }

            if (request.RevenueType.HasValue)
            {
                query = query.Where(p => p.RevenueType == request.RevenueType).ToList();
            }

            int totalItem = query.Count;
            var lists = query
                    .ApplySortAndPaging(request)
                    .ToList();


            var userIds = lists.Select(l => l.UserId).Distinct().ToList();
            if (userIds.Any())
            {
                var studentResults = await _userService.GetStudentsByIdsAsync(userIds);
                var students = studentResults.Content?.Result;

                // Code sau khi Optimize
                var studentLookup = students?
                                    .Where(x => x.Human != null && x.Human.UserId.HasValue)
                                    .ToDictionary(x => x.Human!.UserId!.Value, x => x.Human);
                lists.ForEach(p =>
                {
                    if (studentLookup != null && studentLookup.TryGetValue(p.UserId, out var human))
                    {
                        p.Email = human?.Email;
                        p.FullName = human?.FullName;
                    }
                });
            }

            methodResult.Result = new PagingItemsModel<SearchOrderModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
