// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Hangfire.Application.Services.TrainingServices
{
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Refit;

    public interface ITrainingService
    {
        [Post("/teacher/class-live/approve-auto")]
        Task<IApiResponse<MethodResult<bool>>> ApproveAutoAsync();
    }
}
