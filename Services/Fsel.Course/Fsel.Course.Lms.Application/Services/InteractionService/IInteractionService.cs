// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.InteractionService
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Lms.Application.Services.InteractionService.CommandModels;
    using Fsel.Course.Lms.Application.Services.InteractionService.Models;
    using Fsel.Shared.Enums;
    using Microsoft.AspNetCore.Mvc;
    using Refit;

    public interface IInteractionService
    {
        [Post("/v1/interaction-action/actions")]
        Task<IApiResponse<MethodResult<IList<InteractionActionModel>>>> GetsActionAsync([Body] InteractionActionCommandModel command);

        [Get("/v1/review/student-reviews")]
        Task<IApiResponse<MethodResult<IList<StudentReviewModel>>>> GetStudentReviewsAsync([Query] EnumReviewType reviewType);

        [Get("/v1/interaction-action/execute-list-query")]
        Task<IApiResponse<MethodResult<IList<InteractionActionModel>>>> ExecuteListActionQueryAsync([Query] BaseQueryModel query);

        [Get("/v1/comment/execute-list-query")]
        Task<IApiResponse<MethodResult<IList<CommentModel>>>> ExecuteListCommentQueryAsync([Query] BaseQueryModel query);

        [Post("/v1/interaction-action/aggregate-number-of-likes-and-comments")]
        Task<IApiResponse<MethodResult<AggregateNumberOfLikesAndCommentsModels>>> AggregateNumberOfLikesAndComments([Body] AggregateNumberOfLikesAndCommentsQueryModel query);

        [Get("/v1/customerSurvey/IsCompleted/{id}")]
        Task<IApiResponse<MethodResult<bool>>> IsSurveyCompleted([FromRoute] Guid id);

        //[Post("/v1/customerSurvey")]
        //Task<IApiResponse<MethodResult<IList<CustomerSurveyModel>>>> CreateSurveyAsync([FromBody] CreateCustomerSurveyCommandModel model);

        [Get("/v1/customerSurvey/check-survey")]
        Task<IApiResponse<MethodResult<bool>>> CheckSurveyBySurveyFormType([FromQuery] CheckSurveyBySurveyFormTypeModel query);

        [Post("/v1/harmful-content/harmful-content-image-body")]
        Task<IApiResponse<MethodResult<bool>>> CheckHarmfulContentImage([FromBody] CheckHarmfulContentImageModel query);
    }
}
