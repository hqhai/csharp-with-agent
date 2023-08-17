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
        Task<IApiResponse<MethodResult<bool>>> IsCheckStatusUser([FromQuery] IsCheckPaymentStatusByUserModel query);
    }
}
