// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;

    public class AnswerVideoModel : AnswerModel
    {
        public EnumAnswerStatus? Status { get; set; }

    }
}
