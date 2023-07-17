// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;

    public class ForbiddenWordModel : BaseModel
    {
        public string? Word { get; set; }

        public string? Description { get; set; }
    }
}
