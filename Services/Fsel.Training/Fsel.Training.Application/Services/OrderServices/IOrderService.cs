// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Services.OrderServices
{
    using Fsel.Common.ActionResults;
    using Fsel.Training.Application.Services.OrderServices.Model;
    using Refit;

    public interface IOrderService
    {
        [Get("/v1/package")]
        Task<IApiResponse<MethodResult<IList<PackageModel>>>> GetPackages();
    }
}
