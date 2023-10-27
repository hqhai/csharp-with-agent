// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Services.OrderServices
{
    using Fsel.Common.ActionResults;
    using Fsel.System.Application.Services.OrderServices.Model;
    using Refit;

    public interface IOrderService
    {
        [Get("/package")]
        Task<IApiResponse<MethodResult<IList<PackageModel>>>> GetPackages();
    }
}
