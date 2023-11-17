// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;
    using Fsel.Shared.Enums;

    public class LeaderBoardModel
    {
        public Guid Id { get; set; }
        public int DisplayOrder { get; set; }
        public string? AvatarPath { get; set; }
        public string? FullName { get; set; }
        public double TotalScore { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }
    }
}
