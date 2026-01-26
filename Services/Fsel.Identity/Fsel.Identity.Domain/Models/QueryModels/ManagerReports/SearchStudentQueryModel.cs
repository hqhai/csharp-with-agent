// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.QueryModels.ManagerReports
{
    using System.Text.Json.Serialization;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;

    public class SearchStudentQueryModel : BaseQueryModel
    {
        public string? ListSchoolGrade { get; set; }
        public string? ListSchoolClass { get; set; }
        public string? ListDistrict { get; set; }
        public string? SchoolIdsStr { get; set; }
        public string? ListProvince { get; set; }
        public string? ListStudentId { get; set; }
        public string? ListLearningStatus { get; set; }
        public Guid? ProgramId { get; set; }
        public Guid? SubjectId { get; set; }
        public Guid? LevelId { get; set; }
        public string? LevelIdStr { get; set; }

        [JsonIgnore]
        public IList<string>? SchoolClasses => ListSchoolClass.ToList<string>();

        [JsonIgnore]
        public IList<Guid>? LevelIds => LevelIdStr.ToList<Guid>();

        [JsonIgnore]
        public IList<string>? SchoolGrades => ListSchoolGrade.ToList<string>();

        [JsonIgnore]
        public IList<Guid>? SchoolIds => SchoolIdsStr.ToList<Guid>();

        [JsonIgnore]
        public IList<Guid>? DistrictIds => ListDistrict.ToList<Guid>();

        [JsonIgnore]
        public IList<Guid>? ProvinceIds => ListProvince.ToList<Guid>();

        [JsonIgnore]
        public IList<Guid>? StudentIds => ListStudentId.ToList<Guid>();

        [JsonIgnore]
        public IList<EnumLearningStatus>? LearningStatuses => ListLearningStatus.ToList<EnumLearningStatus>();

        public bool? IsLearning { get; set; }
        public bool IsCheckDate { get; set; }
    }
}
