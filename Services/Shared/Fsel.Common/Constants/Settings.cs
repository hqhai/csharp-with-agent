// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Common.Constants
{
    public static class Settings
    {
        public const string CommandBackgroundTaskDefaultRoute = "api/cmd/bkg-tasks/v{version:apiVersion}/[controller]";

        public const string ReadBackgroundTaskDefaultRoute = "api/read/bkg-tasks/v{version:apiVersion}/[controller]";

        public const string CommandAPIDefaultRoute = "api/cmd/v{version:apiVersion}/[controller]";

        public const string ReadAPIDefaultRoute = "api/read/v{version:apiVersion}/[controller]";

        public const string AggregatorAPIDefaultRoute = "api/aggr/v{version:apiVersion}/[controller]";

        public const string APIVersion = "1.0";

        public const string APIDefaultRoute = "api/v{version:apiVersion}";

        public const string GetAllAPIDefaultRoute = "getall";

        public const string GetByIdAPIDefaultRoute = "get";

        public const string CreateAPIDefaultRoute = "create";

        public const string UpdateAPIDefaultRoute = "update";

        public const string DeleteAPIDefaultRoute = "delete";

        public const int RequireMaxOrderNo = -1;

        public const string SettingFileName = "appsettings.json";

        public const string SurveyQuestionFileName = "Resources//SurveyQuestions.json";

        public const string DefaultConnection = "DefaultConnection";

        public const string PostgreConnection = "PostgreConnection";

        public const string CorsPolicy = "CorsPolicy";
    }
}
