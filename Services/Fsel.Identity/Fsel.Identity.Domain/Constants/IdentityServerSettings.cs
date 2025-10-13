// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Constants
{
    public static class IdentityServerSettings
    {
        public static class AllowedScopes
        {
            public const string Api = "api";
            public const string Roles = "roles";
        }

        public static class JwtApiClaimNames
        {
            public const string Code = "code";
            public const string Status = "status";
            public const string ClassId = "class_id";
            public const string ClassCode = "class_code";
            public const string IsPlacementTest = "is_placement_test";
            public const string IsOrder = "is_order";
            public const string IsSurvey = "is_survey";
            public const string SchoolId = "SchoolId";
        }
    }
}
