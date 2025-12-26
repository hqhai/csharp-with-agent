// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.V1i2
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;

    public class ResultModel : BaseModel
    {
        public Guid StudentId { get; set; }
        public EnumResultStatus Status { get; set; }
    }
}
