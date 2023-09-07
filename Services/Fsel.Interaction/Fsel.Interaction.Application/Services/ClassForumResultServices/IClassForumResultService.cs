// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Services.ClassForumResultServices
{
    using Fsel.Common.ActionResults;
    using Fsel.Interaction.Application.Services.ClassForumResultServices.Models;
    using Microsoft.AspNetCore.Mvc;
    using Refit;

    public interface IClassForumResultService
    {
        [Get("/class-forum-result/{id}")]
        Task<IApiResponse<MethodResult<ClassForumResultModel>>> GetClassForumResultByIdAsync([FromRoute] Guid id);

    }
}
