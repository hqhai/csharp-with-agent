// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class ClassForumAIModel
    {
        private const string DeterminationYES = "YES";
        private const string DeterminationPARTIAL = "PARTIAL";

        public string? SuccessCriteriaItem { get; set; }
        public string? SuccessCriteriaItemDetermination { get; set; }
        public IList<string> SuccessCriteriaItemEvidence { get; set; } = new List<string>();
        public IList<string> SuccessCriteriaItemFix { get; set; } = new List<string>();
        public string? EncouragementCriteriaItem { get; set; }

        public int Score
        {
            get
            {
                return SuccessCriteriaItemDetermination == DeterminationYES ? 2 : SuccessCriteriaItemDetermination == DeterminationPARTIAL ? 1 : default;
            }
        }
    }
}
