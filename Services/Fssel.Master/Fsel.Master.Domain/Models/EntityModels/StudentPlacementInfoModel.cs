// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Master.Domain.Models.EntityModels
{
    using Fsel.Master.Domain.Models.Enums;

    public class StudentPlacementInfoModel
    {
        public Guid StudentId { get; set; }

        public Guid CompetitionEventId { get; set; }

        public Guid? ProvinceId { get; set; }

        public Guid? DistrictId { get; set; }

        public Guid? SchoolId { get; set; }

        public Guid? LevelId { get; set; }
        public string? LevelName { get; set; }

        public EnumResultStatus? Status { get; set; }

        public Guid? ProgramId { get; set; }

        public Guid? SubjectId { get; set; }

        public string? SubjectName { get; set; }
    }
}
