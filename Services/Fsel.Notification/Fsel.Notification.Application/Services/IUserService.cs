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
        [Post("/user/get-users-by-ids")]
        Task<IApiResponse<MethodResult<IList<HumanModel>>>> GetUsersByIdsAsync([Body] GetUsersByIdsQueryModel model);

        [Get("/user/get-user-by-id")]
        Task<IApiResponse<MethodResult<HumanModel>>> GetUserByIdAsync([Query] string? id);

        [Get("/user/get-users-by-role")]
        Task<IApiResponse<MethodResult<IList<UserModel>>>> GetUserByRoleAsync([Query] GetUsersByRoleQueryModel query);
    }
}
