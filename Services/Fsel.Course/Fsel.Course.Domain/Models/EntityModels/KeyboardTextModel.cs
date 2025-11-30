// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;

    public class KeyboardTextModel : BaseModel
    {
        public string? Name { get; set; }
        public EnumVectorLibrary VectorLibrary { get; set; }
        public int Unicode { get; set; }
        public string? FilePath { get; set; }
    }
}
