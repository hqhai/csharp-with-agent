// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Services.OrderServices
{
    using Fsel.Common.ActionResults;
    using Fsel.System.Application.Services.OrderServices.Model;
    using Refit;

    public interface IOrderService
    {
        [Get("/v1/package")]
        Task<IApiResponse<MethodResult<IList<PackageModel>>>> GetPackages();
    }
}
