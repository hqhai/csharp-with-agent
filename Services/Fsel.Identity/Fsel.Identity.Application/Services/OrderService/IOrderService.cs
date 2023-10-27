// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.OrderService
{
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Application.Services.OrderService.Model;
    using Fsel.Shared.Enums;
    using Microsoft.AspNetCore.Mvc;
    using Refit;

    public interface IOrderService
    {
        [Get("/package")]
        Task<IApiResponse<MethodResult<List<PackageModel>>>> GetPackages();

        [Post("/user-referral")]
        Task<IApiResponse<MethodResult<bool>>> CreateUserReferralAsync([Body] CreateUserReferralCommandModel command);

        [Get("/order/get-status")]
        Task<IApiResponse<MethodResult<EnumOrderStatus?>>> GetStatusAsync([FromQuery] GetStatusByUserCommandModel query);

        [Post("/order")]
        Task<IApiResponse<MethodResult<OrderModel>>> CreateOrder([Body] CreateOrderCommandModel command);

        [Put("/admin/order/change-status-order")]
        Task<IApiResponse<MethodResult<bool>>> ChangeStatusOrder([Body] ChangeStatusOrderCommandModel command);
    }
}
