// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Constants
{
    /// <summary>
    /// Constants cho AI Schema types
    /// </summary>
    public static class AIConstant
    {
        /// <summary>
        /// Schema type cho structured outputs
        /// </summary>
        public const string JsonSchemaType = "json_schema";

        /// <summary>
        /// Schema name cho grading/evaluation (ClassForum, etc.)
        /// </summary>
        public const string CriteriaSchema = "criteria_schema";

        /// <summary>
        /// Schema name cho content moderation/determination
        /// </summary>
        public const string DeterminationSchema = "determination_schema";

        /// <summary>
        /// Default AI model name
        /// </summary>
        public const string DefaultModel = "gpt-4o";
    }
}
