namespace Fsel.ExamPractice.Lms.Application.Services.UserServices
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.ExamPractice.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Constants;
    using Microsoft.AspNetCore.Mvc;
    using Refit;

    public interface IUserService
    {
        [RefitCache(CacheSettings.TimeCache.OneHour)]
        [Get("/v1/student/get-by-user-id/{id}")]
        Task<IApiResponse<MethodResult<StudentModel>>> GetStudentByUserIdWithCacheAsync([FromRoute] Guid id);

        [RefitCache(CacheSettings.TimeCache.OneHour)]
        [Get("/v1/user/get-by-student-id/{id}")]
        Task<IApiResponse<MethodResult<StudentModel>>> GetUserByStudentIdWithCache([FromRoute] Guid id);
    }
}
