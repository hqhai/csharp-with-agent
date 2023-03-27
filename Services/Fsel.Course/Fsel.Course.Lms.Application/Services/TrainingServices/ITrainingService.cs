// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.TrainingServices
{
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums;
    using Fsel.Course.Lms.Application.Services.TrainingServices.Models;
    using Refit;

    public interface ITrainingService
    {
        [Get("/class/get-class-new")]
        Task<IApiResponse<MethodResult<IList<ClassModel>>>> GetTrainingByStatusNewAsync();

        [Get("/class/get-new-class-code")]
        Task<IApiResponse<MethodResult<string>>> GetNewTrainingCodeAsync([Body] EnumCourseLevel enumCourseLevel);

        [Post("/class/create-class")]
        Task<IApiResponse<MethodResult<ClassModel>>> CreateTrainingByCheckId([Body] CreateClassStudentModel command);
    }
}
