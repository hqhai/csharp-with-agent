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
        [Post("/v1/class/register-class")]
        Task<IApiResponse<MethodResult<ClassModel>>> RegisterClassAsync([Body] RegisterClassCommandModel command);

        [Delete("/v1/class/delete-student-from-class/{id}")]
        Task<IApiResponse<MethodResult<bool>>> DeleteStudentFromClass([FromRoute] Guid id);

        [Get("/v1/class/get-new-class-by-student-id/{id}")]
        Task<IApiResponse<MethodResult<ClassModel>>> GetNewClassByStudentId([FromRoute] Guid id);

        [Put("/v1/admin/class/active-class/{id}")]
        Task<IApiResponse<MethodResult<bool>>> ActiveClass([FromRoute] Guid id);

        [Put("/v1/admin/class/update-student-status-in-class/{id}")]
        Task<IApiResponse<MethodResult<bool>>> UpdateStatusStudentInClass([FromRoute] Guid id);

        [Post("/v1/class/add-student-into-class")]
        Task<IApiResponse<MethodResult<Guid>>> AddStudentIntoClass([Body] AddStudentIntoClassCommandModel command);
    }
}
