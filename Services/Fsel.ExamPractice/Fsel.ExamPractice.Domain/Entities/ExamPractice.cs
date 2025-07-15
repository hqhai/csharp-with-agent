namespace Fsel.ExamPractice.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;
    using Fsel.ExamPractice.Domain.Enums;

    public class ExamPractice : Entity
    {
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Name { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(50, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Code { get; set; }

        public EnumExamPracticeType Type { get; set; }

        [MaxLength(100, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? SchoolGrade { get; set; }

        public EnumExamPracticeSubType SubType { get; set; }

        [NotMapped]
        public string? SchoolYear
        {
            get
            {
                return StartDate.HasValue && EndDate.HasValue ? StartDate.Value.Year + " - " + EndDate.Value.Year : null;
            }
        }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [MaxLength(100, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Province { get; set; }

        public Guid? ProvinceId { get; set; }

        public int? ExecutionTime { get; set; }

        public EnumExamPracticeStatus Status { get; set; }
        public Shared.Enums.EnumCourseLevel? CourseLevel { get; set; }

        public DateTime? ActivatedAt { get; set; }
        public Guid? ParentExamPracticeId { get; set; }
        public ExamPractice? ParentExamPractice { get; set; }
        public ICollection<ExamPracticeSection> ExamPracticeSections { get; set; } = new List<ExamPracticeSection>();
        public ICollection<ExamPracticeRetry> ExamPracticeRetrys { get; set; } = new List<ExamPracticeRetry>();
        public ICollection<ExamPracticeResult> ExamPracticeResults { get; set; } = new List<ExamPracticeResult>();
    }
}
