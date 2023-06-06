// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;

    public class ExtraPracticeVideoModel : BaseModel
    {
        public Guid ExtraPracticeId { get; set; }
        public Guid VideoId { get; set; }
    }
}
