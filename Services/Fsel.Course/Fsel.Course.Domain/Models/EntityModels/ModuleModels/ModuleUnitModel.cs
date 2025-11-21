// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.ModuleModels
{
    using Fsel.Course.Domain.Enums;

    public class ModuleUnitModel : BaseModuleModel
    {
        public EnumUnitConfigType UnitConfigType { get; set; }
        public Guid UnitId { get; set; }
    }
}
