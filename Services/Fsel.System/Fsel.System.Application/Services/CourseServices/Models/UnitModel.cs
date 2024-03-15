// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Services.CourseServices.Models
{
    using Fsel.Core.Base.BaseModels;

    public class UnitModel : BaseModel
    {
        public string? Name { get; set; }
        public string? Code { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
    }
}
