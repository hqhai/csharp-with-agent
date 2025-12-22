// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.OrderServices
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Lms.Application.Services.OrderServices.Model;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Microsoft.AspNetCore.Mvc;
    using Refit;

    public interface IOrderService
    {
        [Get("/v1/package")]
        Task<IApiResponse<MethodResult<IList<PackageModel>>>> GetPackages();

        [Get("/v1/order/get-status")]
        Task<IApiResponse<MethodResult<EnumOrderStatus?>>> GetStatusAsync([Query] GetStatusByUserCommandModel command);

        [Get("/v1.1/order")]
        Task<IApiResponse<MethodResult<OrderModel>>> GetOrderAsync();

        [Get("/v1.1/order/trial/{userId}")]
        Task<IApiResponse<MethodResult<OrderModel>>> GetOrderTrialAsync([FromRoute] Guid userId);

        [Get("/v1/order/get-current-status/{id}")]
        Task<IApiResponse<MethodResult<EnumTrialRegistrationStatus?>>> GetCurrentStatusAsync([FromRoute] Guid id);

        [Post("/v1/order")]
        Task<IApiResponse<MethodResult<OrderModel>>> CreateOrder([Body] CreateOrderCommandModel command);

        [Post("/v1.2/order/get-users-has-order-payment")]
        Task<IApiResponse<MethodResult<IList<Guid>>>> GetUsersHasOrderPayment([Body] GetUsersHasOrderPaymentModel model);

        [Get("/v1.2/order/get-order-revenue")]
        Task<IApiResponse<MethodResult<PagingItemsModel<SearchOrderModel>>>> GetOrderRevenuesAsync();
    }
}
