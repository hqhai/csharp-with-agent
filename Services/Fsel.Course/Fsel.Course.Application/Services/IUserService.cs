// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Refit;

    public interface IUserService
    {
        [Post("/teacher/get-by-ids")]
        Task<IApiResponse<MethodResult<IList<TeacherModel>>>> GetTeacherByIdsAsync([Body] GetTeacherByIdsQueryModel command);
    }

    public class GetTeacherByIdsQueryModel
    {
        public IList<Guid>? Ids { get; set; }
    }

    public class TeacherModel : BaseModel
    {
        public string? PassportPath { get; set; }

        public string? UniversityDegreePath { get; set; }

        public string? CertificationPath { get; set; }

        public string? PoliceClearancePath { get; set; }

        public Guid HumanId { get; set; }

        public HumanModel? Human { get; set; }
    }

    public class HumanModel
    {
        public string? FullName { get; set; }
    }
}