// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Master.Domain.Models.EntityModels
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using Fsel.Master.Domain.Entities;

    public class PlacementTestStudentModel
    {
        public Guid StudentId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? SkillScoresJson { get; set; }

        public IList<SkillScore>? SkillScores
        {
            get
            {
                return !string.IsNullOrEmpty(SkillScoresJson) ? JsonSerializer.Deserialize<IList<SkillScore>>(SkillScoresJson) : null;
            }
        }

        public Guid? ProgramId { get; set; }
        public Guid? ProvinceId { get; set; }
        public string? Province { get; set; }
        public Guid? DistrictId { get; set; }
        public string? District { get; set; }
        public Guid? SchoolId { get; set; }
        public string? School { get; set; }
        public Guid? CompetitionEventId { get; set; }
        public Guid? LevelId { get; set; }
        public string? LevelName { get; set; }
    }
}
