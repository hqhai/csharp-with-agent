namespace Fsel.ExamPractice.Domain.Enums
{
    using System.ComponentModel;

    public enum EnumExamPracticeType
    {
        [Description("Đề thi Quốc gia")]
        ExamPractice = 0,

        [Description("IELTS Mock Test")]
        IELTS = 1
    }
}
