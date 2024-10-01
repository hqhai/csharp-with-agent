// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    using System;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;

    public class StudentRankingModel : BaseModel
    {
        public Guid StudentId { get; set; }

        public Guid? UserId { get; set; }

        public int DailyStreak { get; set; }

        public double TotalScore { get; set; }

        public int PositionChange { get; set; }

        public int CurrentPosition { get; set; }

        private string? _avatarPath;
        public string? AvatarPath
        {
            set { _avatarPath = value; }
            get { return _avatarPath.AddS3BaseUrl(); }
        }

        public string? FullName { get; set; }

        public string? Email { get; set; }

        public EnumCourseLevel Level { get; set; }

        public EnumCourseType CourseType { get; set; }

        //Tham gia cuộc thi
        public string? SchoolName { get; set; }

        public string? Grade { get; set; }

        public double? Process { get; set; }

        public double? OverallScore { get; set; }

        public DateTime? CompetitionEndDate { get; set; }

        public double? RankingScore { get; set; }

        public Guid CourseResultId { get; set; }

        public WeekEvent? WeekEvent { get; set; }

    }
}
