// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System.Globalization;

    public class ClassForumAIModel
    {
        private const string DeterminationYES = "YES";
        private const string DeterminationPARTIAL = "PARTIAL";

        public string? SuccessCriteriaItem { get; set; }
        public string? SuccessCriteriaItemDetermination { get; set; }
        public object? SuccessCriteriaItemEvidence { get; set; }
        public object? SuccessCriteriaItemFix { get; set; }
        public string? EncouragementCriteriaItem { get; set; }

        public int Score
        {
            get
            {
                return SuccessCriteriaItemDetermination?.ToLower(CultureInfo.CurrentCulture) == DeterminationYES.ToLower(CultureInfo.CurrentCulture) ? 2 : SuccessCriteriaItemDetermination?.ToLower(CultureInfo.CurrentCulture) == DeterminationPARTIAL.ToLower(CultureInfo.CurrentCulture) ? 1 : default;
            }
        }
    }
}
