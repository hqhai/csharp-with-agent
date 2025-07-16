// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Application.Services.SystemServices
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.ExamPractice.Application.Services.SystemServices.Models;
    using Fsel.ExamPractice.Application.Services.SystemServices.QueryModels;
    using Refit;

    public interface ISystemService
    {
        [Get("/v1/location/get-by-ids")]
        Task<IApiResponse<MethodResult<IList<SchoolModel>>>> GetLocationByIdsAsync([Query] GetLocationsByIdsQueryModel query);
    }
}
