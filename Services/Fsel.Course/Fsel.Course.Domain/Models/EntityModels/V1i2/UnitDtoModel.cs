// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.V1i2
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Models.EntityModels.ModuleModels;

    public class UnitDtoModel : BaseModel
    {
        public string? Name { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
        public Guid OriginalId { get; set; }
        public int Version { get; set; }
        public ResultModel? Result { get; set; }
        public IList<ModuleUnitModel>? ModuleUnits { get; set; }
    }
}
