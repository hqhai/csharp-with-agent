// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;

    public class RubyScopeModel : BaseModel
    {
        public string HostType { get; set; } = default!;
        public Guid HostId { get; set; }
        public string FieldKey { get; set; } = default!;
        public string? BaseTextHash { get; set; }
        public int? BaseLengthGraphemes { get; set; }
        public byte[] RowVersion { get; set; } = default!;
    }
}
