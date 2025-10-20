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
    using Fsel.Ordering.Application.Services.UserService.Models;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.EntityModels.V1i2;
    using Fsel.Ordering.Domain.Models.QueryModels.Oders.V1i2;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    public class SearchOrderQuery : SearchOrderQueryModel, IRequest<MethodResult<PagingItemsModel<SearchOrderModel>>>
    {
    }

    public class SearchOrderQueryHandler : IRequestHandler<SearchOrderQuery, MethodResult<PagingItemsModel<SearchOrderModel>>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly ILogger<SearchOrderQuery> _logger;
        private const int BatchSize = 10000;

        public SearchOrderQueryHandler(IOrderRepository orderRepository, AuthContext authContext, IUserService userService, ILogger<SearchOrderQuery> logger)
        {
            _orderRepository = orderRepository;
            _authContext = authContext;
            _userService = userService;
            _logger = logger;
        }

        public async Task<MethodResult<PagingItemsModel<SearchOrderModel>>> Handle(SearchOrderQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<SearchOrderModel>>();

            var query = _orderRepository.Queryable.Where(p => !p.IsTrial).Select(x => new SearchOrderModel
            {
                Id = x.Id,
                Code = x.Code,
                Email = x.Email,
                FullName = x.FullName,
                PhoneNumber = x.PhoneNumber,
                UserId = x.UserId,
                CreatedDate = x.CreatedDate,
                UpdatedDate = x.UpdatedDate,
                CreatedFullName = x.CreatedFullName,
                Status = x.Status,
                Address = x.Address,
                PaymentMethod = x.PaymentMethod,
                PackageId = x.PackageId,
                MonthNumber = x.Package == null ? null : x.Package.MonthNumber,
                RevenueType = x.RevenueType,
                TotalPrice = x.TotalPrice,
                Price = x.Price,
                DiscountPrice = x.DiscountPrice,
                DistrictId = x.DistrictId,
                ProvinceId = x.ProvinceId
            });

            if (request.IsNew.HasValue && request.IsNew == true)
            {
                query = query.Where(p => p.Status == EnumOrderStatus.New && (p.PaymentMethod == EnumPaymentMethodStatus.BankTransfer || p.PaymentMethod == EnumPaymentMethodStatus.Card));
            }
            else if (request.IsNew.HasValue && request.IsNew == false)
            {
                query = query.Where(p => p.Status != EnumOrderStatus.New || (p.Status == EnumOrderStatus.New && (p.PaymentMethod == EnumPaymentMethodStatus.Payoo || p.PaymentMethod == EnumPaymentMethodStatus.AppStore || p.PaymentMethod == EnumPaymentMethodStatus.CHPlay)));
            }

            if (_authContext.Roles?.FirstOrDefault() == EnumRole.Student.ToString() || _authContext.Roles?.FirstOrDefault() == EnumRole.StudentCampus.ToString())
            {
                query = query.Where(p => p.UserId == _authContext.CurrentUserId);
            }

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                if (request.Keyword.IsValidEmail())
                {
                    query = query.Where(p => !string.IsNullOrEmpty(p.Email) && p.Email == request.Keyword);
                }
                else if (request.Keyword.IsValidPhoneNumber())
                {
                    query = query.Where(p => !string.IsNullOrEmpty(p.PhoneNumber) && p.PhoneNumber == request.Keyword);
                }
                else
                {
                    var codeQuery = query.Where(m => m.Code != null && m.Code.Contains(request.Keyword));
                    var fullNameQuery = query.Where(m => m.FullName != null && m.FullName.Contains(request.Keyword));
                    query = codeQuery.Union(fullNameQuery);
                }
            }

            if (request.PackageIds != null && request.PackageIds.Count > 0)
            {
                request.PackageIds = request.PackageIds.Distinct().ToList();
                query = query.Where(p => p.PackageId.HasValue && request.PackageIds.Contains(p.PackageId.Value));
            }

            if (request.StartDate.HasValue && request.EndDate.HasValue)
            {
                query = query.Where(p => p.CreatedDate.HasValue && request.StartDate.Value.Date <= p.CreatedDate.Value.Date && request.EndDate.Value.Date >= p.CreatedDate.Value.Date);
            }
            else if (request.StartDate.HasValue)
            {
                query = query.Where(p => p.CreatedDate.HasValue && request.StartDate.Value.Date <= p.CreatedDate.Value.Date);
            }
            else if (request.EndDate.HasValue)
            {
                query = query.Where(p => p.CreatedDate.HasValue && request.EndDate.Value.Date >= p.CreatedDate.Value.Date);
            }

            if (request.RevenueType.HasValue)
            {
                query = query.Where(p => p.RevenueType == request.RevenueType);
            }

            int totalItem = await query.CountAsync(cancellationToken);
            var lists = await query
                    .ApplySortAndPaging(request)
                    .ToListAsync(cancellationToken);

            var userIds = lists.Select(l => l.UserId).Distinct().ToList();

            var orderUsers = await _orderRepository.Queryable.Where(p => !p.IsTrial && p.RevenueType == EnumPaymentRevenueType.Revenue)
                                                             .WhereBulkContains(userIds, x => x.UserId)
                                                             .GroupBy(x => x.UserId)
                                                             .ToDictionaryAsync(x => x.Key, x => x.Count(), cancellationToken);
            if (userIds.Any())
            {
                var batches = SplitList(userIds, BatchSize);

                var students = new List<StudentModel>();
                foreach (var batch in batches)
                {
                    var studentResults = await _userService.GetStudentsByIdsAsync(batch);
                    if (!studentResults.IsSuccessStatusCode)
                    {
                        methodResult.AddError(studentResults.Error);
                        return methodResult;
                    }
                    else
                    {
                        if (studentResults.Content?.Result != null && studentResults.Content.Result.Count > 0)
                        {
                            students.AddRange(studentResults.Content.Result.ToList());
                        }
                    }
                }

                var studentDict = students
                    .Where(x => x.Human != null && x.Human.UserId.HasValue)
                    .ToDictionary(x => x.Human?.UserId ?? default, x => x);

                if (studentDict != null)
                {
                    lists.ForEach(p =>
                    {
                        if (studentDict.TryGetValue(p.UserId, out var student))
                        {
                            p.StudentCode = student.Human?.Code;
                            p.StudentPhoneNumber = student.Human?.PhoneNumber;
                            p.StudentEmail = student.Human?.Email;
                            p.StudentFullName = student.Human?.FullName;
                            p.ExpiredDate = student.ExpiredDate;
                        }
                        if (orderUsers.TryGetValue(p.UserId, out var countOrder))
                        {
                            p.CountOrder = countOrder;
                        }
                    });
                }
            }

            methodResult.Result = new PagingItemsModel<SearchOrderModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private static List<List<Guid>> SplitList(List<Guid> userIds, int batchSize)
        {
            return userIds.Select((x, i) => new { Index = i, Value = x })
                         .GroupBy(x => x.Index / batchSize)
                         .Select(g => g.Select(x => x.Value).ToList())
                         .ToList();
        }
    }
}
