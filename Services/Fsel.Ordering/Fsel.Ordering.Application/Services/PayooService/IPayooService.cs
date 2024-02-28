// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.PayooService
{
    using System.Threading.Tasks;
    using Fsel.Ordering.Application.Services.PayooService.Models;
    using Refit;

    public interface IPayooService
    {
        [Post("/v1/create-preorder")]
        Task<IApiResponse<PayooModel>> Create([Body] CreatePayooModel model);
    }
}
