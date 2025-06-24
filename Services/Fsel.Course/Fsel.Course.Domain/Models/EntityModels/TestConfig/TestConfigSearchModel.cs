// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Entities;

    public class TestConfigSearchModel : BaseModel
    {
        public string? Name { get; set; }
        public string? Code { get; set; }
        public bool IsActive { get; set; }

        //Program
        public Category? Program { get; set; }
    }
}
