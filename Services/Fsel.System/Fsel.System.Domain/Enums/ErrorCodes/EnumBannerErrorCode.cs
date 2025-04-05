// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Enums.ErrorCodes
{
    public enum EnumBannerErrorCode
    {
        StartDateGreaterThanEndDate,

        ThisStudentViewedThisBanner,

        BannerNotExist,
        DisplayDatesNotNull,
        DisplayDateOutOfRange,
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
        UrlNotNull,
        CustomFrequencyRequiresDisplayDate,
        DisplayStartDateGreaterThanDisplayEndDate,
        CompetitionEventIdsRequired,
        StartDateLowerThanDateTimeNow,
        MaximumBannerPerDay,
        NotTimeDisplayBanner
    }
}
