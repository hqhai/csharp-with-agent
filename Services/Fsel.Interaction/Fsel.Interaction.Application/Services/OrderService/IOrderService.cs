// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Services.OrderService
{
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Enums;
    using Microsoft.AspNetCore.Mvc;
    using Refit;

    public interface IOrderService
    {
        [Get("/v1/order/get-current-status/{id}")]
        Task<IApiResponse<MethodResult<EnumTrialRegistrationStatus?>>> GetCurrentStatusAsync([FromRoute] Guid id);
    }
}
