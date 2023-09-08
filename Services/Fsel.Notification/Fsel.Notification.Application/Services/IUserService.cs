// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Notification.Application.Services
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Notification.Application.Services.Models;
    using Fsel.Notification.Application.Services.UserServices;
    using Microsoft.AspNetCore.Mvc;
    using Refit;

    public interface IUserService
    {
        [Get("/student/get-by-user-id/{id}")]
        Task<IApiResponse<MethodResult<StudentModel>>> GetStudentByUserIdAsync([FromRoute] Guid id);

        [Get("/user/get-users-by-role")]
        Task<IApiResponse<MethodResult<IList<UserModel>>>> GetUserByRoleAsync([Query] GetUsersByRoleQueryModel query);
    }
}
