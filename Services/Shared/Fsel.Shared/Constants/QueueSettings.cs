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
                public const string AggregateDataStudentsInEvent = $"{KeyQueue}_AggregateDataStudentsInEvent";
                public const string CreateStudentsAndParentsFromFile = $"{KeyQueue}_CreateStudentsAndParentsFromFile";
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
                public const string Banner = $"{KeyQueue}_Banner";
                public const string BannerRealTime = $"{KeyQueue}_BannerRealTime";
                public const string SpeechToTextRealTime = $"{KeyQueue}_SpeechToTextRealTime";
                public const string DictionaryRealTime = $"{KeyQueue}_DictionaryRealTime";

                public const string SetTimeExamPractice = $"{KeyQueue}_SetTimeExamPractice";
                public const string GetTimeExamPractice = $"{KeyQueue}_GetTimeExamPractice";
                public const string ExamPracticeSpeaking = $"{KeyQueue}_ExamPracticeSpeaking";
                public const string ExamPracticeWriting = $"{KeyQueue}_ExamPracticeWriting";
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
                public const string SpeechToTextAi = $"{KeyQueue}_SpeechToTextAi";
                public const string ResponseSpeechToTextPendingAi = $"{KeyQueue}_ResponseSpeechToTextPendingAi";
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
                public const string SendWeeklyReport = $"{KeyQueue}_SendWeeklyReport";
                public const string AggregateDataWeeklyReport = $"{KeyQueue}_AggregateDataWeeklyReport";
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
                public const string ExportExcelStudentLearningProcess = $"{KeyQueue}_ExportExcelStudentLearningProcess";
                public const string SavePlacementTestAnswers = $"{KeyQueue}_SavePlacementTestAnswers";
                public const string ErrorExplainGgSheet = $"{KeyQueue}_ErrorEplainGgSheet";
                public const string ExportExcelSchoolLearningProcess = $"{KeyQueue}_ExportExcelSchoolLearningProcess";
                public const string SpeechToTextPendingAi = $"{KeyQueue}_SpeechToTextPendingAi";
                public const string PushNotice = $"{KeyQueue}_PushNotice";
                public const string ExportExcelUserInformationSupportSale = $"{KeyQueue}_ExportExcelUserInformationSupportSale";
                public const string JobStudentAggregate = $"{KeyQueue}_JobStudentAggregate";
                public const string NotifyWeeklyReportCourseTarget = $"{KeyQueue}_NotifyWeeklyReportCourseTarget";
                public const string NotifyWeeklyCourseGoalTarget = $"{KeyQueue}_NotifyWeeklyCourseGoalTarget";
            }
        }

        public static class ExamPracticeQueue
        {
            public const string KeyQueue = nameof(ExamPracticeQueue);

                public const string SubmitQuestionShortAnswerWordBase = $"{KeyQueue}_SubmitQuestionShortAnswerWordBase";
                public const string ClassForumPronunciationAi = $"{KeyQueue}_ClassForumPronunciationAi";
            public static class NameQueue
            {
                public const string GetTimeExamPractice = $"{KeyQueue}_GetTimeExamPractice";
                public const string SetTimeExamPractice = $"{KeyQueue}_SetTimeExamPractice";
                public const string ExamPracticeAnwserResponse = $"{KeyQueue}_ExamPracticeAnwserResponse";
                public const string SpeakingAI = $"{KeyQueue}_SpeakingAI";
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
                public const string BuyBlindBox = $"{KeyQueue}_BuyBlindBox";
                public const string SendNotifyBuyBlindBox = $"{KeyQueue}_SendNotifyBuyBlindBox";
                public const string ChooseDailyQuizWinners = $"{KeyQueue}_ChooseDailyQuizWinners";
                public const string SendDictionary = $"{KeyQueue}_SendDictionary";
                public const string CrawDictionaryData = $"{KeyQueue}_CrawDictionaryData";
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
                public const string SaveUserSurveyAssignment = $"{KeyQueue}_SaveUserSurveyAssignment";
                public const string SendNotifyUserHasSurvey = $"{KeyQueue}_SendNotifyUserHasSurvey";
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
                public const string AddCoinWhenCoursePurchased = $"{KeyQueue}_AddCoinWhenCoursePurchased";
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
