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

        BannerScopesRequired,

        EachBannerOnlyOneCourseLevel,

        TargetUsersRequired,

        ApplicableUserGroupsRequired,

        CompetitionEventIdsRequired,

        MaximumPerDayMustGreaterThanZero,

        DisplayIntervalTimeMustGreaterThanZero,

        CodeAlreadyExist
    }
}
