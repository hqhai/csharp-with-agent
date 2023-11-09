// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.OrderServices
{
    using Fsel.Common.ActionResults;
    using Fsel.Course.Lms.Application.Services.OrderServices.Model;
    using Fsel.Shared.Enums;
    using Refit;

    public interface IOrderService
    {
        [Get("/package")]
        Task<IApiResponse<MethodResult<IList<PackageModel>>>> GetPackages();

        [Get("/order/get-status")]
        Task<IApiResponse<MethodResult<EnumOrderStatus?>>> GetStatusAsync([Query] GetStatusByUserCommandModel command);

        [Post("/order")]
        Task<IApiResponse<MethodResult<OrderModel>>> CreateOrder([Body] CreateOrderCommandModel command);
    }
}
