// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Enums
{
    using System.ComponentModel;

    public enum EnumPracticeMode
    {
        [Description("Chế độ luyện tập")]
        Practice,

        [Description("Chế độ mô phỏng bài thi")]
        ExamSimulation
    }
}
