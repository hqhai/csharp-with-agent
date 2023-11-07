// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Constants
{
    public static class QueueSettings
    {
        public static class AuthQueue
        {
            public const string KeyQueue = nameof(AuthQueue);

            public static class NameQueue
            {
            }
        }

        public static class UserQueue
        {
            public const string KeyQueue = nameof(UserQueue);

            public static class NameQueue
            {
                public const string SyncStudentShieldEveryDay = $"{KeyQueue}_SyncStudentShieldEveryDay";
                public const string UpdateStudentsDailyStreak = $"{KeyQueue}_UpdateStudentsDailyStreak";
                public const string LeaderBoard = $"{KeyQueue}_LeaderBoard";
            }
        }

        public static class RealtimeQueue
        {
            public const string KeyQueue = nameof(RealtimeQueue);

            public static class NameQueue
            {
                public const string DiscussionBoard = $"{KeyQueue}_DiscussionBoard";
                public const string LeaderBoard = $"{KeyQueue}_LeaderBoard";
                public const string ClassForum = $"{KeyQueue}_ClassForum";
            }
        }

        public static class SenderQueue
        {
            public const string KeyQueue = nameof(SenderQueue);

            public static class NameQueue
            {
            }
        }

        public static class StorageQueue
        {
            public const string KeyQueue = nameof(StorageQueue);

            public static class NameQueue
            {
            }
        }

        public static class LcmsQueue
        {
            public const string KeyQueue = nameof(LcmsQueue);

            public static class NameQueue
            {
            }
        }

        public static class LmsQueue
        {
            public const string KeyQueue = nameof(LmsQueue);

            public static class NameQueue
            {
                public const string QuestBoardMainFinish = $"{KeyQueue}_QuestBoardMainFinish";
                public const string SendNotification = $"{KeyQueue}_SendNotification";
                public const string OrderCreateNotification = $"{KeyQueue}_OrderCreateNotification";
                public const string UpdateTeacherGradingInClassForumAndMockTest = $"{KeyQueue}_UpdateTeacherGradingInClassForumAndMockTest";
                public const string UpdateOcCheckInClassForumResult = $"{KeyQueue}_UpdateOcCheckInClassForumResult";
                public const string CompleteTestWhenTimeOut = $"{KeyQueue}_CompleteTestWhenTimeOut";
                public const string SetTimeToCompleteTest = $"{KeyQueue}_SetTimeToCompleteTest";
                public const string DeleteClassForumByFlag = $"{KeyQueue}_DeleteClassForumByFlag";
            }
        }

        public static class InteractionQueue
        {
            public const string KeyQueue = nameof(InteractionQueue);

            public static class NameQueue
            {
                public const string SendNotification = $"{KeyQueue}_SendNotification";
                public const string InteractionAction = $"{KeyQueue}_InterationAction";
            }
        }

        public static class OrderingQueue
        {
            public const string KeyQueue = nameof(OrderingQueue);

            public static class NameQueue
            {
                public const string SendNotification = $"{KeyQueue}_SendNotification";
            }
        }

        public static class NotificationQueue
        {
            public const string KeyQueue = nameof(NotificationQueue);

            public static class NameQueue
            {
                public const string DiscussionBoard = $"{KeyQueue}_DiscussionBoard";
                public const string Notification = $"{KeyQueue}_Notification";
            }
        }

        public static class TrainingQueue
        {
            public const string KeyQueue = nameof(TrainingQueue);

            public static class NameQueue
            {
                public const string UpdateClassLiveAssignment = $"{KeyQueue}_UpdateClassLiveAssignment";
            }
        }
    }
}