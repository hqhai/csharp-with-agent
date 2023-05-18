// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.ITrainingService
{
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Application.Services.TrainingServices.Models;
    using Microsoft.AspNetCore.Mvc;
    using Refit;

    public interface ITrainingService
    {
        [Get("/class/get-class-by-student/{studentId}")]
        Task<IApiResponse<MethodResult<ClassModel>>> GetClassByStudentId([FromRoute] Guid studentId);
    }
}
