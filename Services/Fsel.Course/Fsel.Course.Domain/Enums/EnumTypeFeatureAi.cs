// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Enums
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    public enum EnumTypeFeatureAi
    {
        [Description("Fluency and Coherence(Lưu loát và mạch lạc)"), Display(Name = "Fluency and Coherence")]
        Fc,
        [Description("Lexical Resource(Tài nguyên từ vựng)"), Display(Name = "Lexical Resource")]
        Lr,
        [Description("Grammatical Range and Accuracy(Độ chính xác ngữ pháp)"), Display(Name = "Grammatical Range and Accuracy")]
        Gra,
        [Description("Task Achivement(Hoàn thành nhiệm vụ)"), Display(Name = "Task Achivement")]
        Ta,
        [Description("Coherence and Cohesion(Mạch lạc và gán kết)"), Display(Name = "Coherence and Cohesion")]
        Cc,
    }
}
