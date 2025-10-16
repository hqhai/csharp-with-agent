// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Enums
{
    using System.ComponentModel;

    public enum EnumTypeFeatureAi
    {
        [Description("Fluency and Coherence(Lưu loát và mạch lạc)")]
        Fc,
        [Description("Lexical Resource(Tài nguyên từ vựng)")]
        Lr,
        [Description("Grammatical Range and Accuracy(Độ chính xác ngữ pháp)")]
        Gra,
        [Description("Task Achivement(Hoàn thành nhiệm vụ)")]
        Ta,
        [Description("Coherence and Cohesion(Mạch lạc và gán kết)")]
        Cc,
    }
}
