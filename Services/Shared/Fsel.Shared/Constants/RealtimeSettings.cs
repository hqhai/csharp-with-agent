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

        public static class ClassForumAIFeedBackHub
        {
            public const string Pattern = $"/class-forum-detail-result";

            public static class Methods
            {
                public const string ClassForumResultFeedBack = $"ClassForumResultFeedBack";
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
    }
}
