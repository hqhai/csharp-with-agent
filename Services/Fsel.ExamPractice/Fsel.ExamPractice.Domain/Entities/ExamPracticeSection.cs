// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Entities;
    using Fsel.ExamPractice.Domain.Entities.Configs;
    using Fsel.ExamPractice.Domain.Enums;
    using Fsel.Shared.Enums;

    public class ExamPracticeSection : Entity
    {
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Name { get; set; }

        public EnumCourseSkill? CourseSkill { get; set; }

        /// <summary>
        /// Config
        /// </summary>
        public string? ConfigStr { get; set; }

        [NotMapped]
        public SectionMediaConfig? Config
        {
            get { return ConvertHelper.Deserialize<SectionMediaConfig>(ConfigStr); }
            set { ConfigStr = ConvertHelper.Serialize(value); }
        }

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

        public EnumSectionExamPracticeType? Type { get; set; }
        public int DisplayOrder { get; set; }
        public Guid? ExamPracticeId { get; set; }
        public ExamPractice? ExamPractice { get; set; }
        public Guid? ParentExamPracticeSectionId { get; set; }
        public ExamPracticeSection? ParentExamPracticeSection { get; set; }
        public ICollection<ExamPracticeSection> ExamPracticeSections { get; set; } = new List<ExamPracticeSection>();
        public ICollection<ExamPracticeSectionResult> ExamPracticeSectionResults { get; set; } = new List<ExamPracticeSectionResult>();
        public ICollection<Question> Questions { get; set; } = new List<Question>();
        public ICollection<ExamPracticeAISetting> ExamPracticeAISettings { get; set; } = new List<ExamPracticeAISetting>();
        public ICollection<ExamPracticeAnswer> ExamPracticeAnswers { get; set; } = new List<ExamPracticeAnswer>();
        public ICollection<ExamPracticeScore> ExamPracticeScores { get; set; } = new List<ExamPracticeScore>();
    }
}
