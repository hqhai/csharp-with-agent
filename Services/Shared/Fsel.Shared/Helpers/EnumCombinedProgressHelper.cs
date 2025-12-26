// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Helpers
{
    using Fsel.Shared.Enums;

    public static class EnumCombinedProgressHelper
    {
        public static EnumProgressStatus GetProgressStatusFromCounts(int completed, int target)
        {
            if (completed > target)
            {
                return EnumProgressStatus.Ahead;
            }
            if (completed < target)
            {
                return EnumProgressStatus.Behind;
            }
            return EnumProgressStatus.OnTrack;
        }

        public static EnumCombinedProgress CurrentCombineProgress(EnumProgressStatus totalStatus, EnumProgressStatus weekStatus) =>
           (totalStatus, weekStatus) switch
           {
               (EnumProgressStatus.Ahead, EnumProgressStatus.Ahead) => EnumCombinedProgress.TotalAheadWeekAhead,
               (EnumProgressStatus.Ahead, EnumProgressStatus.OnTrack) => EnumCombinedProgress.TotalAheadWeekOnTrack,
               (EnumProgressStatus.Ahead, EnumProgressStatus.Behind) => EnumCombinedProgress.TotalAheadWeekBehind,
               (EnumProgressStatus.OnTrack, EnumProgressStatus.Ahead) => EnumCombinedProgress.TotalOnTrackWeekAhead,
               (EnumProgressStatus.OnTrack, EnumProgressStatus.OnTrack) => EnumCombinedProgress.TotalOnTrackWeekOnTrack,
               (EnumProgressStatus.OnTrack, EnumProgressStatus.Behind) => EnumCombinedProgress.TotalOnTrackWeekBehind,
               (EnumProgressStatus.Behind, EnumProgressStatus.Ahead) => EnumCombinedProgress.TotalBehindWeekAhead,
               (EnumProgressStatus.Behind, EnumProgressStatus.OnTrack) => EnumCombinedProgress.TotalBehindWeekOnTrack,
               (EnumProgressStatus.Behind, EnumProgressStatus.Behind) => EnumCombinedProgress.TotalBehindWeekBehind,
               _ => EnumCombinedProgress.TotalOnTrackWeekOnTrack
           };

        public static EnumCombinedProgress CombineProgress(EnumProgressStatus totalStatus) =>
           totalStatus switch
           {
               EnumProgressStatus.Ahead => EnumCombinedProgress.TotalAheadWeekBehind,
               EnumProgressStatus.OnTrack => EnumCombinedProgress.TotalOnTrackWeekBehind,
               EnumProgressStatus.Behind => EnumCombinedProgress.TotalBehindWeekBehind,
           };

        public static EnumCombinedProgress GetCurrentCombineProgress(int totalCompleted, int totalTarget, EnumProgressStatus weekStatus)
        {
            var totalStatus = GetProgressStatusFromCounts(totalCompleted, totalTarget);
            return CurrentCombineProgress(totalStatus, weekStatus);
        }

        public static EnumCombinedProgress GetCombineProgress(int totalCompleted, int totalTarget)
        {
            var totalStatus = GetProgressStatusFromCounts(totalCompleted, totalTarget);
            return CombineProgress(totalStatus);
        }
    }
}
