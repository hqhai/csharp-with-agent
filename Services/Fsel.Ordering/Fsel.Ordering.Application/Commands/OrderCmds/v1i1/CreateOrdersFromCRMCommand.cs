// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.OrderCmds.v1i1
{
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.CommandModels.Orders.V1i1;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateOrdersFromCRMCommand : CreateOrdersFromCRMCommandModels, IRequest<MethodResult<bool>>
    {
    }

    public class CreateOrdersFromCRMCommandHandler : IRequestHandler<CreateOrdersFromCRMCommand, MethodResult<bool>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IPackageRepository _packageRepository;

        public CreateOrdersFromCRMCommandHandler(IOrderRepository orderRepository, IPackageRepository packageRepository)
        {
            _orderRepository = orderRepository;
            _packageRepository = packageRepository;
        }

        public async Task<MethodResult<bool>> Handle(CreateOrdersFromCRMCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();
            if (request.UsersInfo == null || request.UsersInfo.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var orders = new List<Order>();

            var packages = await _packageRepository.Queryable.ToListAsync(cancellationToken);

            foreach (var item in request.UsersInfo)
            {
                var package = packages.First(p => p.Code == item.PackageCode);

                var isHaveOrderTrial = await _orderRepository.Queryable.AnyAsync(p => p.IsTrial && p.Status == EnumOrderStatus.Payment && p.UserId == item.UserId, cancellationToken);

                var order = AddDataIntoOrder(item, package, isHaveOrderTrial);
                if (!order.IsValid())
                {
                    methodResult.AddError(order.ErrorMessages);
                    return methodResult;
                }
                orders.Add(order);
            }

            await _orderRepository.ExecuteTransactionAsync(async () =>
            {
                await _orderRepository.AddList(orders);
                await _orderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = true;
                return methodResult;
            });
            return methodResult;
        }

        private static Order AddDataIntoOrder(CreateOrdersFromCRMCommandModel userInfo, Package package, bool isAddExpireDate = false)
        {
            var order = new Order();
            order.FullName = userInfo.FullName;
            order.Email = userInfo.Email;
            order.Status = EnumOrderStatus.Payment;
            order.UserId = userInfo.UserId;
            order.Code = RandomCodeOrder(package, userInfo.StudentCode);
            order.Price = package.Price;
            order.DiscountPercent = (int)(userInfo.DiscountPercent.HasValue ? userInfo.DiscountPercent : 0);
            order.DiscountPrice = (decimal)NumberHelper.ConvertDoublePercent(Convert.ToDouble(order.Price * order.DiscountPercent));
            order.TotalPrice = order.Price - order.DiscountPrice;
            order.PaymentMethod = EnumPaymentMethodStatus.BankTransfer;
            order.ExpireDate = isAddExpireDate ? DateTime.UtcNow.AddMonths(package.MonthNumber) : null;
            order.OrderTransactions = new List<OrderTransaction>()
            {
                new OrderTransaction()
                {
                    Status = EnumOrderTransactionStatus.Success,
                    Type = EnumOrderTransactionType.BankTransfer,
                    RequestBodyStr = "CRM"
                }
            };
            return order;
        }

        private static string RandomCodeOrder(Package package, string? studentCode)
        {
            CultureInfo culture = new CultureInfo("en-US");
            string formattedDate = DateTime.UtcNow.ToString("ddMMyyyyHHmmss", culture);
            var code = $"{"CRM"}{formattedDate}{package.Code.ToString()!.Substring(0, 1)}{studentCode}";
            return code;
        }
    }
}
