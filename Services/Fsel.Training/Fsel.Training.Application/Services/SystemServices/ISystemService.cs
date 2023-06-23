// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Services.SystemServices
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Training.Application.Services.SystemServices.Model;
    using Refit;

    public interface ISystemService
    {
        [Post("/live-time-frame/get-list-live-time-frame-by-ids")]
        Task<IApiResponse<MethodResult<LiveTimeFrameModel>>> GetTimeFramByIdsAsync([Body] IList<Guid> ids);
    }
}
