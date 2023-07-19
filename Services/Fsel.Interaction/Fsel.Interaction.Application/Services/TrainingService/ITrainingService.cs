// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Services.TrainingService
{
    using Fsel.Common.ActionResults;
    using Fsel.Interaction.Application.Services.TrainingService.Models;
    using Microsoft.AspNetCore.Mvc;
    using Refit;

    public interface ITrainingService
    {
        [Get("/class/get-class-by-student/{studentId}")]
        Task<IApiResponse<MethodResult<ClassModel>>> GetClassByStudentId([FromRoute] Guid studentId);

        [Get("/class/class-course-student/{studentId}")]
        Task<IApiResponse<MethodResult<IList<ClassStudentInfoModel>>>> GetClassCourseStudentAsync([FromRoute] Guid studentId);
    }
}
