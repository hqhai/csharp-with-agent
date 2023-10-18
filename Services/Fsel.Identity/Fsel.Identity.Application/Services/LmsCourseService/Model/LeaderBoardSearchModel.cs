// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.LmsCourseService.Model
{
    using Fsel.Shared.Enums;

    public class LeaderBoardSearchModel
    {
        public IList<LeaderBoardModel>? LeaderBoards { get; set; }
        public LeaderBoardModel? LeaderBoard { get; set; }
    }

    public class LeaderBoardModel
    {
        public Guid Id { get; set; }
        public int DisplayOrder { get; set; }
        public string? AvatarPath { get; set; }
        public string? FullName { get; set; }
        public int DailyStreak { get; set; }
        public double TotalScore { get; set; }
        public Guid UserId { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
    }
}
