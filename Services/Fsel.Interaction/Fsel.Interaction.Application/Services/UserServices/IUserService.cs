// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Services.UserServices
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Interaction.Application.Services.UserServices.Models;
    using Microsoft.AspNetCore.Mvc;
    using Refit;

    public interface IUserService
    {
        [Post("/student/get-by-user-ids")]
        Task<IApiResponse<MethodResult<IList<StudentModel>>>> GetStudentByUserIdsAsync([Body] IList<Guid> ids);

        [Get("/student/get-by-user-id/{id}")]
        Task<IApiResponse<MethodResult<StudentModel>>> GetStudentByUserIdAsync([FromRoute] Guid id);

        [Post("/student/get-by-student-ids")]
        Task<IApiResponse<MethodResult<IList<StudentModel>>>> GetStudentsByStudentIdsAsync([Body] IList<Guid> studentIds);

        [Post("/user/get-users-by-ids")]
        Task<IApiResponse<MethodResult<IList<HumanModel>>>> GetUsersByIdsAsync([Body] GetUsersByIdsQueryModel model);

        [Post("/student/execute-list-query")]
        Task<IApiResponse<MethodResult<IList<StudentModel>>>> ExecuteListStudentQueryAsync([Body] BaseQueryModel query);
    }
}
