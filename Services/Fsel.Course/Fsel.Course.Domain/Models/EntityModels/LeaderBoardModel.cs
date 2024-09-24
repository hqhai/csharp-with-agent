// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;
    using Fsel.Common.Helpers;
    using Fsel.Shared.Enums;

    public class LeaderBoardModel
    {
        public Guid Id { get; set; }
        public int DisplayOrder { get; set; }
        private string? _avatarPath;

        public string? AvatarPath
        {
            set { _avatarPath = value; }
            get { return _avatarPath.AddS3BaseUrl(); }
        }

        public string? FullName { get; set; }
        public double TotalScore { get; set; }

        public EnumCourseLevel? CourseLevel { get; set; }
    }
}
