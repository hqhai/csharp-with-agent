// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.InteractionService
{
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Microsoft.AspNetCore.Mvc;
    using Refit;

    public interface IInteractionService
    {
        [Get("/v1/customerSurvey/IsCompleted/{id}")]
        Task<IApiResponse<MethodResult<bool>>> IsSurveyCompleted([FromRoute] Guid id);

        [Get("/v1/surveyQuestion/{userId}")]
        Task<IApiResponse<MethodResult<IList<StudentSurveyQuestionModel>>>> SurveyQuestionsByUserId([FromRoute] Guid userId);

        //[Post("/v1/customerSurvey")]
        //Task<IApiResponse<MethodResult<IList<CustomerSurveyModel>>>> CreateSurvey([FromBody] CreateCustomerSurveyCommandModel model);

        [Delete("/admin/student/delete-student/{id}")]
        Task<IApiResponse<MethodResult<bool>>> DeleteListDataUser([FromRoute] Guid id);
    }
}
