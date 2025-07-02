// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Notification.Application.Services
{
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Notification.Application.Services.Models;
    using Fsel.Notification.Application.Services.UserServices;
    using Refit;

    public interface IUserService
    {
        [Post("/v1/user/get-users-by-ids")]
        Task<IApiResponse<MethodResult<IList<UserModel>>>> GetUsersByIdsAsync([Body] GetUsersByIdsQueryModel model);

        [Get("/v1/user/get-user-by-id")]
        Task<IApiResponse<MethodResult<UserModel>>> GetUserByIdAsync([Query] string? id);

        [Get("/v1/user/get-users-by-role")]
        Task<IApiResponse<MethodResult<IList<UserModel>>>> GetUserByRoleAsync([Query] GetUsersByRoleQueryModel query);
    }
}
