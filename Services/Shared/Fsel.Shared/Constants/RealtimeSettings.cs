// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Constants
{
    public static class RealtimeSettings
    {
        public static class SendDictionaryHub
        {
            public const string Pattern = $"/get-dictionary";

            public static class Methods
            {
                public const string GetDictionary = $"GetDictionary";
            }
        }

        public static class SendNotifyBuyBlindBoxHub
        {
            public const string Pattern = $"/buy-blind-box";

            public static class Methods
            {
                public const string BuyBlindBox = $"BuyBlindBox";
            }
        }

        public static class SendStudentsFromFileHub
        {
            public const string Pattern = $"/send-students-from-file";

            public static class Methods
            {
                public const string SendStudentsFromFile = $"SendStudentsFromFile";
            }
        }

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

        public static class ClassForumAIFeedBackHub
        {
            public const string Pattern = $"/class-forum-detail-result";

            public static class Methods
            {
                public const string ClassForumResultFeedBack = $"ClassForumResultFeedBack";
            }
        }

        public static class TestWritingAIFeedBackHub
        {
            public const string Pattern = $"/test-writing";

            public static class Methods
            {
                public const string TestWritingAIFeedBackHub = $"TestWritingAIFeedBackHub";
            }
        }

        public static class TestSpeakingAIFeedBackHub
        {
            public const string Pattern = $"/test-speaking";

            public static class Methods
            {
                public const string TestSpeakingAIFeedBack = $"TestSpeakingAIFeedBack";
            }
        }

        public static class MockTestWritingAIFeedBackHub
        {
            public const string Pattern = $"/mock-test-writing";

            public static class Methods
            {
                public const string MockTestWritingAIFeedBack = $"MockTestWritingAIFeedBack";
            }
        }

        public static class MockTestSpeakingAIFeedBackHub
        {
            public const string Pattern = $"/mock-test-speaking";

            public static class Methods
            {
                public const string MockTestSpeakingAIFeedBack = $"MockTestSpeakingAIFeedBack";
            }
        }

        public static class SetTimeModuleHub
        {
            public const string Pattern = $"/set-time-module";

            public static class Methods
            {
                public const string SetTimeModule = $"SetTimeModule";
            }
        }

        public static class FeatureAccessTimeHub
        {
            public const string Pattern = $"/feature-access-time";

            public static class Methods
            {
                public const string FeatureAccessTime = $"FeatureAccessTime";
            }
        }

        public static class ChatBotHub
        {
            public const string Pattern = $"/chat-bot";

            public static class Methods
            {
                public const string ChatBot = $"ChatBot";
            }
        }

        public static class TechieHub
        {
            public const string Pattern = $"/techie";

            public static class Methods
            {
                public const string Techie = $"TechieHub";
            }
        }

        public static class PaymentHub
        {
            public const string Pattern = $"/payment";

            public static class Methods
            {
                public const string Payment = $"Payment";
            }
        }

        public static class BannerHub
        {
            public const string Pattern = $"/banner";

            public static class Methods
            {
                public const string Banner = $"Banner";
            }
        }

        public static class TranscriptHub
        {
            public const string Pattern = $"/transcript";

            public static class Methods
            {
                public const string Transcript = $"Transcript";
            }
        }

        public static class ExamPracticeSpeakingAIFeedBackHub
        {
            public const string Pattern = $"/exam-practice-speaking";

            public static class Methods
            {
                public const string ExamPracticeSpeakingAIFeedBack = $"ExamPracticeSpeakingAIFeedBack";
            }
        }

        public static class ExamPracticeWritingAIFeedBackHub
        {
            public const string Pattern = $"/exam-practice-writing";

            public static class Methods
            {
                public const string ExamPracticeWritingAIFeedBack = $"ExamPracticeWritingAIFeedBack";
            }
        }

        public static class SetTimeExamPracticeHub
        {
            public const string Pattern = $"/set-time-exam-practice";

            public static class Methods
            {
                public const string SetTimeExamPracticeHub = $"SetTimeExamPracticeHub";
            }
        }

        public static class QuestionTypeHub
        {
            public const string Pattern = $"/question-type";

            public static class Methods
            {
                public const string QuestionType = $"QuestionType";
            }
        }

        public static class TranslationHub
        {
            public const string Pattern = $"/translation";

            public static class Methods
            {
                public const string TranslationResultHub = $"TranslationResultHub";
            }
        }

        public static class SpeechToTextHub
        {
            public const string Pattern = $"/speech-to-text";

            public static class Methods
            {
                public const string OnRecognizing = $"OnRecognizing";
                public const string OnRecognized = $"OnRecognized";
                public const string OnSessionStarted = $"OnSessionStarted";
                public const string OnSessionStopped = $"OnSessionStopped";
                public const string OnStarted = $"OnStarted";
                public const string OnStopped = $"OnStopped";
                public const string OnError = $"OnError";
                public const string OnAudioSaved = $"OnAudioSaved";
            }
        }
    }
}
