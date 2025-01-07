// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Enums.ErrorCodes
{
    public enum EnumBannerErrorCode
    {
        StartDateGreaterThanEndDate,

        ThisStudentViewedThisBanner,

        BannerNotExist,

        CustomFrequencyRequiresDisplayDate,

        DisplayStartDateGreaterThanDisplayEndDate,

        DisplayStartTimeGreaterThanDisplayEndTime,

        DisplayStartTimeMustGreaterThanZero,

        EachBannerOnlyOneCourseLevel,

        TargetUsersRequired,

        ApplicableUserGroupsRequired,

        CompetitionEventIdNotNull,

        MaximumPerDayMustGreaterThanZero,

        DisplayIntervalTimeMustGreaterThanZero,

        CodeAlreadyExist,

        BannerImageNotNull,

        ContentNotNull,

        BannerScopeNotNull,

        CourseLevelNotNull,

        TargetUserNotNull,

        FilePathNotNull,

        UrlNotNull
    }
}
