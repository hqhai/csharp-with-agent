// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.TrainingService
{
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Application.Services.TrainingService.Models;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Microsoft.AspNetCore.Mvc;
    using Refit;

    public interface ITrainingService
    {
        [Get("/class/get-class-by-student/{studentId}")]
        Task<IApiResponse<MethodResult<ClassModel>>> GetClassByStudentId([FromRoute] Guid studentId);

        [Get("/admin/class/{classId}")]
        Task<IApiResponse<MethodResult<List<Guid>?>>> GetStudentIdsByClassId([FromRoute] Guid classId);

        [Get("/class/class-course-student/{studentId}")]
        Task<IApiResponse<MethodResult<IList<StudentCourseModel>>>> GetClassCourseStudentAsync([FromRoute] Guid studentId);
    }
}
