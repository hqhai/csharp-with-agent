// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.StudentServices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Lms.Application.Services.StudentServices.Models;
    using Refit;

    public interface IStudentService
    {
        [Post("/student/get-by-ids")]
        Task<IApiResponse<MethodResult<StudentModel>>> GetStudentByIdsAsync([Body] GetStudentByIdQueryModel command);
    }
}
