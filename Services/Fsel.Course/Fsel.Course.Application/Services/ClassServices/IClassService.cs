// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Services
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums;
    using Fsel.Core.Base.BaseModels;
    using Refit;

    public interface IClassService
    {
        [Post("/class/get-class-new")]
        Task<IApiResponse<MethodResult<IList<ClassModel>>>> GetClassByStatusNewAsync();

        [Post("/class/get-new-class-code")]
        Task<IApiResponse<MethodResult<string>>> GetNewClassCodeAsync([Body] EnumCourseLevel enumCourseLevel);
    }

    public class ClassModel : BaseModel
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public DateTime TimeStart { get; set; }
        public DateTime TimeEnd { get; set; }
        public EnumClassType Status { get; set; }
    }
}
