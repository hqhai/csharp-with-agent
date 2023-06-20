// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Services.UserServices
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Interaction.Application.Services.UserServices.Models;
    using Refit;

    public interface IUserService
    {
        [Post("/student/get-by-user-ids")]
        Task<IApiResponse<MethodResult<IList<StudentModel>>>> GetStudentByUserIdsAsync([Body] IList<string> ids);
    }
}
