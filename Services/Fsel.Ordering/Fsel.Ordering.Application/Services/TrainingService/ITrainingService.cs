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
    }
}
