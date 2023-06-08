// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.OrderQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Ordering.Application.Services.UserService;
    using Fsel.Ordering.Application.Services.UserService.Models;
    using Fsel.Ordering.Domain.Enums;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Ordering.Domain.Models.QueryModels.Oders;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchOrderQuery : SearchOderQueryModel, IRequest<MethodResult<PagingItemsModel<SearchOrderModel>>>
    {
    }

    public class SearchOrderQueryHandler : IRequestHandler<SearchOrderQuery, MethodResult<PagingItemsModel<SearchOrderModel>>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUserService _userService;

        public SearchOrderQueryHandler(IOrderRepository orderRepository, IUserService userService)
        {
            _orderRepository = orderRepository;
            _userService = userService;
        }

        public async Task<MethodResult<PagingItemsModel<SearchOrderModel>>> Handle(SearchOrderQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<SearchOrderModel>>();

            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
            var orders = _orderRepository.Queryable.Include(p => p.Package).Select(x => new SearchOrderModel
            {
                Id = x.Id,
                UserId = x.UserId.ToString(),
                Code = x.Code,
                CourseId = x.CourseId,
                CreatedDate = x.CreatedDate,
                CreatedFullName = x.CreatedFullName,
                PackageName = x.Package!.Code.ToString(),
                Status = x.Status,
                PaymentMethod = x.PaymentMethod
            });
            if (request.Status.HasValue)
            {
                orders = orders.Where(p => request.Status == false ? p.Status == EnumOrderStatus.New : p.Status != EnumOrderStatus.New);
            }
            int totalItem = await orders.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await orders
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            GetStudentByUserIdsQuery getStudentByUserIdsQuery = new GetStudentByUserIdsQuery()
            {
                UserIds = lists.Select(p => p.UserId).ToList()!,
            };
            var students = await _userService.GetStudentsByIdsAsync(getStudentByUserIdsQuery);
            if (students.IsSuccessStatusCode)
            {
                foreach (var item in lists)
                {
                    item.CourseName = students.Content?.Result?.FirstOrDefault(x => string.Equals(item.UserId, x.UserId, StringComparison.OrdinalIgnoreCase))?.CourseLevel;
                }
            }
            methodResult.Result = new PagingItemsModel<SearchOrderModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
