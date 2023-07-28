// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.OrderService
{
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Application.Services.OrderService.Model;
    using Refit;

    public interface IOrderService
    {
        [Get("/package")]
        Task<IApiResponse<MethodResult<List<PackageModel>>>> GetPackages();

        [Post("/user-referral")]
        Task<IApiResponse<MethodResult<bool>>> CreateUserReferralAsync([Body] CreateUserReferralCommandModel command);
    }
}
