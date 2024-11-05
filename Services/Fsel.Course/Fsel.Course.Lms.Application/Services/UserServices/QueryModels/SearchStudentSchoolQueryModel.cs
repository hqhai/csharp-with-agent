// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.UserServices.QueryModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class SearchStudentSchoolQueryModel : BaseQueryModel
    {
        public string? SchoolGrade { get; set; }
        public string? ListSchoolId { get; set; }
        public string? ListStudentId { get; set; }
        public bool IsCheckDate { get; set; }
        public EnumCompletionStatus? Status { get; set; }
    }
}
