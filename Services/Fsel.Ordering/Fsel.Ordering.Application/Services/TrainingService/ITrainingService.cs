// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.TrainingService
{
    using Fsel.Common.ActionResults;
    using Fsel.Ordering.Application.Services.TrainingService.CommandModels;
    using Fsel.Ordering.Application.Services.TrainingService.Models;
    using Refit;

    public interface ITrainingService
    {
        [Post("/class/register-class")]
        Task<IApiResponse<MethodResult<ClassModel>>> RegisterClassAsync([Body] RegisterClassCommandModel command);
    }
}
