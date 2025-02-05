// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Services.TrainingServices
{
    using Fsel.Common.ActionResults;
    using Fsel.Interaction.Application.Services.TrainingServices.Models;
    using Microsoft.AspNetCore.Mvc;
    using Refit;

    public interface ITrainingService
    {
        [Get("/v1/class/get-class-by-student/{studentId}")]
        Task<IApiResponse<MethodResult<ClassModel>>> GetClassByStudentId([FromRoute] Guid studentId);

        [Get("/v1/class/class-course-student/{studentId}")]
        Task<IApiResponse<MethodResult<IList<ClassStudentInfoModel>>>> GetClassCourseStudentAsync([FromRoute] Guid studentId);

        [Post("/v1/class/classes-by-studentids")]
        Task<IApiResponse<MethodResult<IList<ClassStudentModel>>>> GetClassByStudentIdsAsync([FromBody] GetClassListByStudentIdsModel query);

        [Get("/v1/class/get-class-to-student/{studentId}")]
        Task<IApiResponse<MethodResult<ClassModel>>> GetClassToStudentId([FromRoute] Guid studentId);
    }
}
