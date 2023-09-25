// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Services.TrainingServices.Models
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class ClassStudentInfoModel : BaseModel
    {
        public Guid CourseId { get; set; }
        public string? CourseName { get; set; }
        public string? ClassName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Guid PackageId { get; set; }
        public string? Membership { get; set; }
        public EnumStatusClass Status { get; set; }
    }
}
