// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.TrainingServices
{
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.TrainingServices.Models;
    using Fsel.Shared.Enums;
    using Microsoft.AspNetCore.Mvc;
    using Refit;

    public interface ITrainingService
    {
        [Post("/class/get-class-list-status-new")]
        Task<IApiResponse<MethodResult<IList<CourseClassModel>>>> GetClassListStatusNewAsync([Body] GetClassListStatusNewModel command);

        [Get("/class/get-new-class-code")]
        Task<IApiResponse<MethodResult<string>>> GetNewClassCodeAsync([Query] EnumCourseLevel courseLevel);

        [Post("/class/register-class")]
        Task<IApiResponse<MethodResult<ClassModel>>> RegisterClass([Body] CreateClassStudentModel command);

        [Get("/class/get-class-by-student/{studentId}")]
        Task<IApiResponse<MethodResult<ClassModel>>> GetClassByStudentId([FromRoute] Guid studentId);
    }
}
