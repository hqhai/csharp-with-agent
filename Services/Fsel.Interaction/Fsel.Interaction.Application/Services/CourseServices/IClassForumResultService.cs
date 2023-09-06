// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Services.CourseServices
{
    using Fsel.Common.ActionResults;
    using Fsel.Interaction.Application.Services.CourseServices.Models;
    using Microsoft.AspNetCore.Mvc;
    using Refit;

    public interface IClassForumResultService
    {
        [Get("/class-forum-result/{id}")]
        Task<IApiResponse<MethodResult<CourseModel>>> GetDetailClassForumDetail([FromRoute] Guid id);
        
    }
}
