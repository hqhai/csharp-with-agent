// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Services.UserServices
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Interaction.Application.Services.UserServices.Models;
    using Microsoft.AspNetCore.Mvc;
    using Refit;

    public interface IUserService
    {
        [Get("/student/get-by-ids")]
        Task<IApiResponse<MethodResult<IList<StudentModel>>>> GetStudentByUserIdsAsync([FromRoute] IList<Guid> ids);

        [Get("/student/get-by-user-id/{id}")]
        Task<IApiResponse<MethodResult<StudentModel>>> GetStudentByUserIdAsync([FromRoute] Guid id);
    }
}
