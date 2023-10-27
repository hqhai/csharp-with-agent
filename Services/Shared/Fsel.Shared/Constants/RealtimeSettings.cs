// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Constants
{
    public static class RealtimeSettings
    {
        public static class DiscussionBoardHub
        {
            public const string Pattern = $"/discussion-board";
            public static class Methods
            {
                public const string CommentLikeMessage = $"CommentLikeMessage";
            }
        }

        public static class NotificationHub
        {
            public const string Pattern = $"/notification";
            public static class Methods
            {
                public const string NotificationMessage = $"NotificationMessage";
            }
        }

        public static class LeaderBoardHub
        {
            public const string Pattern = $"/leader-board";
            public static class Methods
            {
                public const string LeaderBoardMessage = $"LeaderBoardMessage";
            }
        }
    }
}
