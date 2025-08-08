// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;

    public class KeyboardLayoutModel : BaseModel
    {
        public string? Name { get; set; }

        public string? Description { get; set; }

        public Guid LanguageId { get; set; }
    }
}
