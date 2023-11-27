// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;

    public class BaseAnswerModel : BaseModel
    {
        public object? Answer { get; set; }
        public int CorrectCount { get; set; }
    }
}
