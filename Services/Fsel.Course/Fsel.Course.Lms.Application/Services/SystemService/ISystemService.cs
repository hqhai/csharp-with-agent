// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.SystemService
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Lms.Application.Services.SystemService.Models;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using Microsoft.AspNetCore.Mvc;
    using Refit;

    public interface ISystemService
    {
        [Post("/v1/course-time-config/execute-list-query")]
        Task<IApiResponse<MethodResult<IList<CourseTimeConfigModel>>>> CourseTimeConfigQueryAsync([Body] BaseQueryModel query);

        [Get("/v1/course-time-config")]
        Task<IApiResponse<MethodResult<PagingItemsModel<CourseTimeConfigModel>>>> GetCourseTimeConfigAsync();

        [Get("/v1/log-action/{userId}")]
        Task<IApiResponse<MethodResult<LogActionDaysModel>>> GetLogActionsByUserId([FromRoute] Guid userId);

        [Post("/v1/log-action")]
        Task<IApiResponse<MethodResult<IList<LogActionDaysModel>>>> GetLogActionsByUserIdsAsync([FromBody] IList<Guid> ids);

        [Post("/v1/feature-access-time/gets")]
        Task<IApiResponse<MethodResult<IList<FeatureAccessTimeModel>>>> GetFeatureAccessTimesAsync([FromBody] FeatureAccessTimesQueryModel query);

        [Post("/v1/feature-access-time/get-to-modules")]
        Task<IApiResponse<MethodResult<IList<FeatureAccessTimeModel>>>> GetFeatureAccessTimeToModulesAsync([FromBody] FeatureAccessTimesQueryModel query);

        [Get("/v1/feature-access-time/get-detail")]
        Task<IApiResponse<MethodResult<FeatureAccessTimeModel>>> GetFeatureAccessTimeAsync([FromQuery] FeatureAccessTimeQueryModel query);

        [Get("/v1/forbidden-word/get-list-forbidden-word")]
        Task<IApiResponse<MethodResult<IList<string>>>> CheckContainForbiddenWord([FromQuery] string Word);

        [Get("/v1/token-config/get-token")]
        Task<IApiResponse<MethodResult<TokenConfigModel>>> GetTokenConfigAsync([Query] GetTokenQueryModel query);

        [Get("/v1/token-config/get-tokens")]
        Task<IApiResponse<MethodResult<IList<TokenConfigModel>>>> GetTokenConfigsAsync([Query] GetTokenConfigsQueryModel query);

        [Get("/v1/feature-access-time/get-feature-access-time-business")]
        Task<IApiResponse<MethodResult<IList<FeatureAccessTimeBusinessModel>>>> GetFeatureAccessTimeBusiness([FromQuery] GetFeatureAccessTimeBusinessQueryModel model);

        [Get("/v1/feature-access-time/execute-list-query")]
        Task<IApiResponse<MethodResult<IList<FeatureAccessTimeModel>>>> GetListFeatureAccessTime([FromQuery] BaseQueryModel model);
    }
}
