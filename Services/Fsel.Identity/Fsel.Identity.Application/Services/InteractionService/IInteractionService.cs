// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.InteractionService
{
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Microsoft.AspNetCore.Mvc;
    using Refit;

    public interface IInteractionService
    {
        [Get("/customerSurvey/IsCompleted/{id}")]
        Task<IApiResponse<MethodResult<bool>>> IsSurveyCompleted([FromRoute] Guid id);
    }
}
