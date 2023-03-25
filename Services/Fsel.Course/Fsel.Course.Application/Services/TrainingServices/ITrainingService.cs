// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Services.TrainingServices
{
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums;
    using Fsel.Course.Application.Services.TrainingServices.Models;
    using Fsel.Course.Domain.Models.CommandModels.Courses;
    using Refit;

    public interface ITrainingService
    {
        [Post("/class/get-class-new")]
        Task<IApiResponse<MethodResult<IList<TrainingModel>>>> GetTrainingByStatusNewAsync();

        [Post("/class/get-new-class-code")]
        Task<IApiResponse<MethodResult<string>>> GetNewTrainingCodeAsync([Body] EnumCourseLevel enumCourseLevel);

        [Post("/class/create-class")]
        Task<IApiResponse<MethodResult<TrainingModel>>> CreateTrainingByCheckId([Body] CreateClassCommandModel command);
    }
}
