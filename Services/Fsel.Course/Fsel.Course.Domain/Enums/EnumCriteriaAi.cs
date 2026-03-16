// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Enums
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    public enum EnumCriteriaAi
    {
        [Description("Fluency and Coherence (Lưu loát và mạch lạc)")]
        [Display(Name = "Fluency and Coherence")]
        Fc,

        [Description("Lexical Resource (Vốn từ vựng)")]
        [Display(Name = "Lexical Resource")]
        Lr,

        [Description("Grammatical Range and Accuracy (Phạm vi và độ chính xác ngữ pháp)")]
        [Display(Name = "Grammatical Range and Accuracy")]
        Gra,

        [Description("Task Achievement (Mức độ hoàn thành yêu cầu đề bài)")]
        [Display(Name = "Task Achievement")]
        Ta,

        [Description("Coherence and Cohesion (Mạch lạc và liên kết)")]
        [Display(Name = "Coherence and Cohesion")]
        Cc,

        [Description("Task Response (Mức độ trả lời đúng và đủ yêu cầu đề bài)")]
        [Display(Name = "Task Response")]
        Tr,
    }
}
