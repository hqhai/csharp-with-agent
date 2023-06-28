// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.TrainingService
{
    using Fsel.Common.ActionResults;
    using Fsel.Ordering.Application.Services.TrainingService.CommandModels;
    using Fsel.Ordering.Application.Services.TrainingService.Models;
    using Microsoft.AspNetCore.Mvc;
    using Refit;

    public interface ITrainingService
    {
        [Post("/class/register-class")]
        Task<IApiResponse<MethodResult<ClassModel>>> RegisterClassAsync([Body] RegisterClassCommandModel command);

        [Delete("/class/delete-student-from-class/{id}")]
        Task<IApiResponse<MethodResult<bool>>> DeleteStudentFromClass([FromRoute] Guid id);

        [Get("/class/get-new-class-by-student-id/{id}")]
        Task<IApiResponse<MethodResult<ClassModel>>> GetNewClassByStudentId([FromRoute] Guid id);

        [Put("/admin/class/active-class/{id}")]
        Task<IApiResponse<MethodResult<bool>>> ActiveClass([FromRoute] Guid id);

    }
}
