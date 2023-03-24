// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Class.Doman.Models.EntityModels
{
    using Fsel.Class.Doman.Enums;
    using Fsel.Core.Base.BaseModels;

    public class ClassModel : BaseModel
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public DateTime TimeStart { get; set; }

        public DateTime TimeEnd { get; set; }

        public EnumClassType Status { get; set; }

        public Guid StudentId { get; set; }
    }
}
