// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.OrderServices
{
    using Fsel.Common.ActionResults;
    using Fsel.Course.Lms.Application.Services.OrderServices.Model;
    using Microsoft.AspNetCore.Mvc;
    using Refit;

    public interface IOrderService
    {
        [Get("/package")]
        Task<IApiResponse<MethodResult<IList<PackageModel>>>> GetPackages();

        [Get("/order/is-status-payment")]
        Task<IApiResponse<MethodResult<bool>>> IsCheckStatusPayment([FromQuery] IsCheckPaymentStatusByUserModel query);

        [Get("/order/is-status-new")]
        Task<IApiResponse<MethodResult<bool>>> IsCheckStatusNew();

        [Post("/order")]
        Task<IApiResponse<MethodResult<OrderModel>>> CreateOrder([Body] CreateOrderCommandModel command);
    }
}
