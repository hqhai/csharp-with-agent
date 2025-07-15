// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.TrainingServices
{
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.TrainingServices.CommandModels;
    using Fsel.Course.Lms.Application.Services.TrainingServices.Models;
    using Fsel.Course.Lms.Application.Services.TrainingServices.QueryModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using Microsoft.AspNetCore.Mvc;
    using Refit;

    public interface ITrainingService
    {
        [Post("/v1/class/get-class-list-status-new")]
        Task<IApiResponse<MethodResult<IList<CourseClassModel>>>> GetClassListStatusNewAsync([Body] GetClassListStatusNewModel command);

        [Get("/v1/class/get-new-class-code")]
        Task<IApiResponse<MethodResult<string>>> GetNewClassCodeAsync([Query] EnumCourseLevel courseLevel);

        [Post("/v1/class/register-class")]
        Task<IApiResponse<MethodResult<ClassModel>>> RegisterClassAsync([Body] RegisterClassCommandModel command);

        [Get("/v1/class/get-class-by-student/{studentId}")]
        Task<IApiResponse<MethodResult<ClassModel>>> GetClassByStudentId([FromRoute] Guid studentId);

        [Post("/v1/class/classes-by-studentids")]
        Task<IApiResponse<MethodResult<IList<ClassStudentModel>>>> GetClassByStudentIdsAsync([FromBody] GetClassListByStudentIdsModel query);

        [Get("/v1/class/get-classes/{studentId}")]
        Task<IApiResponse<MethodResult<IList<ClassModel>>>> GetListClassByStudentIdAsync([FromRoute] Guid studentId);

        [Post("/v1/class/classes-by-studentids/diffirent-course")]
        Task<IApiResponse<MethodResult<IList<CompetitionClassStudentModel>>>> GetListClassBySpecificStudentIdsAsync([FromBody] GetClassListBySpecificStudentIdsModel query);

        [Get("/v1/class/get-classes-by-csoId/{csoId}")]
        Task<IApiResponse<MethodResult<IList<ClassModel>>>> GetClassesByCsoIdAsync([FromRoute] Guid csoId);

        [Get("/v1/class/gets-by-course-ids")]
        Task<IApiResponse<MethodResult<IList<ClassModel>>>> GetsByCourseIdsAsync([FromQuery] GetsByCourseIdsQueryModel query);

        [Get("/v1/class/get-class-to-student/{studentId}")]
        Task<IApiResponse<MethodResult<ClassModel>>> GetClassToStudentIdAsync([FromRoute] Guid studentId);

        [Get("/v1/class/get-students-in-7-day-choose-level")]
        Task<IApiResponse<MethodResult<IList<StudentsIn7DayChooseLevelModel>>>> GetStudentsIn7DayChooseLevel();
    }
}
