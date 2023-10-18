// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.InteractionService
{
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Application.Services.InteractionService.Models;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Microsoft.AspNetCore.Mvc;
    using Refit;

    public interface IInteractionService
    {
        [Get("/customerSurvey/IsCompleted/{id}")]
        Task<IApiResponse<MethodResult<bool>>> IsSurveyCompleted([FromRoute] Guid id);

        [Get("/surveyQuestion/{userId}")]
        Task<IApiResponse<MethodResult<IList<StudentSurveyQuestionModel>>>> SurveyQuestionsByUserId([FromRoute] Guid userId);

        [Post("/customerSurvey")]
        Task<IApiResponse<MethodResult<IList<CustomerSurveyModel>>>> CreateSurvey([FromBody] CreateCustomerSurveyCommandModel model);
    }
}
