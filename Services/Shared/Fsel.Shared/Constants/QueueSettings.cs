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
                public const string UpdateStatusTrialStudent = $"{KeyQueue}_UpdateStatusTrialStudent";
                public const string SetTimeToSendReviewFsel = $"{KeyQueue}_SetTimeToSendReviewFsel";
                public const string SendNotification = $"{KeyQueue}_SendNotification";
                public const string CreateTokenHistory = $"{KeyQueue}_CreateTokenHistory";
                public const string AddExpiredDateForStudent = $"{KeyQueue}_AddExpiredDateForStudent";
                public const string AddFeatureMission = $"{KeyQueue}_AddFeatureMission";
                public const string NoticeExtend = $"{KeyQueue}_NoticeExtend";
                public const string WeeklyNotice = $"{KeyQueue}_WeeklyNotice";
                public const string JobRunEvents = $"{KeyQueue}_JobRunEvents";
                public const string CheckUserDeletion = $"{KeyQueue}_CheckUserDeletion";
                public const string CreateStudentsFromFile = $"{KeyQueue}_CreateStudentsFromFile";
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
                public const string AIFeedBack = $"{KeyQueue}_AIFeedBack";
                public const string FeatureAccessTime = $"{KeyQueue}_FeatureAccessTime";
                public const string ChatBot = $"{KeyQueue}_ChatBot";
                public const string ChatBotRealTime = $"{KeyQueue}_ChatBotRealTime";
                public const string SetTimeModule = $"{KeyQueue}_SetTimeModule";
                public const string GetTimeModule = $"{KeyQueue}_GetTimeModule";
                public const string MockTestWriting = $"{KeyQueue}_MockTestWriting";
                public const string MockTestSpeaking = $"{KeyQueue}_MockTestSpeaking";
                public const string TechieAction = $"{KeyQueue}_TechieAction";
                public const string SendStudentsFromFile = $"{KeyQueue}_SendStudentsFromFile";
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
                public const string QuestBoard = $"{KeyQueue}_QuestBoard";
                public const string SendNotification = $"{KeyQueue}_SendNotification";
                public const string OrderCreateNotification = $"{KeyQueue}_OrderCreateNotification";
                public const string UpdateTeacherGradingInClassForumAndMockTest = $"{KeyQueue}_UpdateTeacherGradingInClassForumAndMockTest";
                public const string UpdateOcCheckInClassForumResult = $"{KeyQueue}_UpdateOcCheckInClassForumResult";
                public const string CompleteTestWhenTimeOut = $"{KeyQueue}_CompleteTestWhenTimeOut";
                public const string SetTimeToCompleteTest = $"{KeyQueue}_SetTimeToCompleteTest";
                public const string SetTimeRetryMockTest = $"{KeyQueue}SetTimeRetryMockTest";
                public const string SetTimeRetryClassForum = $"{KeyQueue}SetTimeRetryClassForum";
                public const string RetryMockTestAction = $"{KeyQueue}RetryMockTestAction";
                public const string RetryClassForumAction = $"{KeyQueue}RetryClassForumAction";
                public const string SetTimeClassForumDone = $"{KeyQueue}_SetTimeClassForumDone";
                public const string DeleteClassForumByFlag = $"{KeyQueue}_DeleteClassForumByFlag";
                public const string ClassForumAIResponse = $"{KeyQueue}_ClassForumAIResponse";
                public const string MockTestAnwserResponse = $"{KeyQueue}_MockTestAnwserResponse";
                public const string WeeklyReport = $"{KeyQueue}_WeeklyReport";
                public const string WeeklySnapShotLeaderBoard = $"{KeyQueue}_WeeklySnapShotLeaderBoard";
                public const string CreateTokenHistory = $"{KeyQueue}_CreateTokenHistory";
                public const string UpdateClassForumResultToExpiredTime = $"{KeyQueue}_UpdateClassForumResultToExpiredTime";
                public const string DisconnectSocketCalculateTime = $"{KeyQueue}_DisconnectSocketCalculateTime";
                public const string DoQuestBoard = $"{KeyQueue}_DoQuestBoard";
                public const string GetTimeModule = $"{KeyQueue}_GetTimeModule";
                public const string SaveUserCourseSetting = $"{KeyQueue}_SaveUserCourseSetting";
                public const string SpeakingAI = $"{KeyQueue}_SpeakingAI";
                public const string Techie = $"{KeyQueue}_Techie";
                public const string StudentRankingEvent = $"{KeyQueue}_StudentRankingEvent";
                public const string RankedStudent = $"{KeyQueue}_RankedStudent";
            }
        }

        public static class SystemQueue
        {
            public const string KeyQueue = nameof(SystemQueue);

            public static class NameQueue
            {
                public const string QuestBoard = $"{KeyQueue}_QuestBoard";
                public const string SetCompleteApprovalPost = $"{KeyQueue}_SetCompleteApprovalPost";
                public const string CompleteApprovalPostTimeOut = $"{KeyQueue}_CompleteApprovalPostTimeOut";
                public const string SetCompleteApprovalPostTimeOut = $"{KeyQueue}_SetCompleteApprovalPostTimeOut";
                public const string ReviewFsel = $"{KeyQueue}_ReviewFsel";
                public const string NoticeAccessTime = $"{KeyQueue}_NoticeAccessTime";
                public const string Techie = $"{KeyQueue}_Techie";
                public const string CreateLuckyTicket = $"{KeyQueue}_CreateLuckyTicket";
            }
        }

        public static class InteractionQueue
        {
            public const string KeyQueue = nameof(InteractionQueue);

            public static class NameQueue
            {
                public const string SendNotification = $"{KeyQueue}_SendNotification";
                public const string InteractionAction = $"{KeyQueue}_InterationAction";
                public const string CreateTokenHistory = $"{KeyQueue}_CreateTokenHistory";
            }
        }

        public static class OrderingQueue
        {
            public const string KeyQueue = nameof(OrderingQueue);

            public static class NameQueue
            {
                public const string SendNotification = $"{KeyQueue}_SendNotification";
                public const string FinishSubmission = $"{KeyQueue}_FinishSubmission";
                public const string NoticePayment = $"{KeyQueue}_NoticePayment";
                public const string JobActiveEvent = $"{KeyQueue}_JobActiveEvent";
                public const string ChangeStatusOrder = $"{KeyQueue}_ChangeStatusOrder";
                public const string JobUpdateVouchersStatus = $"{KeyQueue}_JobUpdateVouchersStatus";
                public const string NoticeExtendPackage = $"{KeyQueue}_NoticeExtendPackage";
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
                public const string SaveUserCourseSetting = $"{KeyQueue}_SaveUserCourseSetting";
            }
        }

        public static class PlantDefenderQueue
        {
            public const string KeyQueue = nameof(PlantDefenderQueue);

            public static class NameQueue
            {
                public const string DeleteGuestStudent = $"{KeyQueue}_DeleteGuestStudent";
            }
        }
    }
}
