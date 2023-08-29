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
            }
        }

        public static class RealtimeQueue
        {
            public const string KeyQueue = nameof(RealtimeQueue);

            public static class NameQueue
            {
                public const string DiscussionBoard = $"{KeyQueue}_DiscussionBoard";
                public const string UpdateClassLiveAssignment = $"{KeyQueue}_UpdateClassLiveAssignment";
                public const string QuestBoardFinishOneLesson = $"{KeyQueue}_QuestBoardFinishOneLesson";
                public const string QuestBoardFinishOneHomeWork = $"{KeyQueue}_QuestBoardFinishOneHomeWork";
                public const string QuestBoardFinishOneUnitTest = $"{KeyQueue}_QuestBoardFinishOneUnitTest";
                public const string QuestBoardFinishOneFinalTest = $"{KeyQueue}_QuestBoardFinishOneFinalTest";
                public const string QuestBoardFinishOneUnit = $"{KeyQueue}_QuestBoardFinishOneUnit";
                public const string QuestBoardFinishOneLevelPass = $"{KeyQueue}_QuestBoardFinishOneLevelPass";
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
            }
        }

        public static class InteractionQueue
        {
            public const string KeyQueue = nameof(InteractionQueue);

            public static class NameQueue
            {
            }
        }

        public static class OrderingQueue
        {
            public const string KeyQueue = nameof(OrderingQueue);

            public static class NameQueue
            {
            }
        }

        public static class SystemQueue
        {
            public const string KeyQueue = nameof(SystemQueue);

            public static class NameQueue
            {
            }
        }

        public static class TrainingQueue
        {
            public const string KeyQueue = nameof(TrainingQueue);

            public static class NameQueue
            {
            }
        }
    }
}
