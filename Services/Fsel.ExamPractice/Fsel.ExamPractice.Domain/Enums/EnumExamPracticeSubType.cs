namespace Fsel.ExamPractice.Domain.Enums
{
    using System.ComponentModel;

    public enum EnumExamPracticeSubType
    {
        [Description("Đề thi Đại học")]
        UniversityEntrance = 0,

        [Description("Đề thi cấp 3")]
        HighschoolEntrance = 1,

        [Description("Đề luyện tập")]
        Practice = 2,

        [Description("Full Mock Test")]
        FullMockTest = 3,

        [Description("Skill Mock Test")]
        SkillMockTest = 4,

        [Description("Full Vstep Skill")]
        FullVstepSkill,

        [Description("Single Vstep Skill")]
        SingleVstepSkill
    }
}
