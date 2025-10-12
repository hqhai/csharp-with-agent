// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.TestConfigs
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class TestSection : Entity
    {
        /// <summary>
        /// Tên Section
        /// </summary>
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Name { get; set; }

        public EnumTestLayoutType? LayoutType { get; set; }

        public double? TotalScore { get; set; }

        public int DisplayOrder { get; set; }

        public string? SubQuestionIndexsStr { get; set; }

        [NotMapped]
        public int SubQuestionNumber
        {
            get { return SubQuestionIndexs?.Count ?? default; }
        }

        [NotMapped]
        public IList<int>? SubQuestionIndexs
        {
            get { return ConvertHelper.Deserialize<IList<int>>(SubQuestionIndexsStr); }
            set { SubQuestionIndexsStr = ConvertHelper.Serialize(value); }
        }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public string? ConfigStr { get; set; }

        [NotMapped]
        public TestSectionConfig? Config
        {
            get { return ConvertHelper.Deserialize<TestSectionConfig>(ConfigStr); }
            set { ConfigStr = ConvertHelper.Serialize(value); }
        }

        public string? ScoringFormulaConfigsStr { get; set; }

        [NotMapped]
        public IList<ScoringFormulaConfig>? ScoringFormulaConfigs
        {
            get { return ConvertHelper.Deserialize<IList<ScoringFormulaConfig>?>(ScoringFormulaConfigsStr); }
            set { ScoringFormulaConfigsStr = ConvertHelper.Serialize(value); }
        }

        [MaxLength(500, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? I18nSkillDescription { get; set; }

        public string? ReportContentBankConfigsStr { get; set; }

        [NotMapped]
        public IList<ReportContentBankConfig>? ReportContentBankConfigs
        {
            get { return ConvertHelper.Deserialize<IList<ReportContentBankConfig>?>(ReportContentBankConfigsStr); }
            set { ReportContentBankConfigsStr = ConvertHelper.Serialize(value); }
        }

        [Range(0, 100, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public double? Percent { get; set; }

        public Skill? Skill { get; set; }
        public Guid? SkillId { get; set; }

        public Test? Test { get; set; }
        public Guid? TestId { get; set; }

        public TestSection? Parent { get; set; }
        public Guid? ParentId { get; set; }
        public ICollection<TestSection> TestSections { get; set; } = new List<TestSection>();
        public ICollection<TestAISetting> TestAISettings { get; set; } = new List<TestAISetting>();
        public ICollection<TestSectionQuestion> TestSectionQuestions { get; set; } = new List<TestSectionQuestion>();
        public ICollection<TestSectionResult> SectionResults { get; set; } = new List<TestSectionResult>();
        public ICollection<TestAnswer> TestAnswers { get; set; } = new List<TestAnswer>();
    }

    public class TestSectionConfig
    {

        [MaxLength(10000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? MediaPost { get; set; }

        public double? ExecutionTime { get; set; }

        [MaxLength(10000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Instruction { get; set; }

        public int TargetWord { get; set; }
        public string? VideoFilePath { get; set; }
        public string? SubFilePath { get; set; }
        public float DisplayTime { get; set; }
    }

    public class ScoringFormulaConfig
    {
        public int From { get; set; }

        public int? Equal { get; set; }
    }

    public class ReportContentBankConfig
    {
        public double From { get; set; }
        public double? To { get; set; }
        public string? SkillLevel { get; set; }
        public string? KeyI18n { get; set; }
    }
}
