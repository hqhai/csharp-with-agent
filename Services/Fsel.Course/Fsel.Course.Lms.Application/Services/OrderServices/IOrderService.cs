// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.OrderServices
{
    using Fsel.Common.ActionResults;
    using Fsel.Course.Lms.Application.Services.OrderServices.Model;
    using Fsel.Shared.Enums;
    using Microsoft.AspNetCore.Mvc;
    using Refit;

    public interface IOrderService
    {
        [Get("/v1/package")]
        Task<IApiResponse<MethodResult<IList<PackageModel>>>> GetPackages();

        [Get("/v1/order/get-status")]
        Task<IApiResponse<MethodResult<EnumOrderStatus?>>> GetStatusAsync([Query] GetStatusByUserCommandModel command);

        [Get("/v1/order/get-current-status")]
        Task<IApiResponse<MethodResult<EnumTrialRegistrationStatus?>>> GetCurrentStatusAsync();

        [Post("/v1/order")]
        Task<IApiResponse<MethodResult<OrderModel>>> CreateOrder([Body] CreateOrderCommandModel command);
    }
}
