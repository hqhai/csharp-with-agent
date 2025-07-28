// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.V1i1
{
    using Fsel.Core.Entities;
    using Enums;
    using IEntities;

    public class UnitModule : Entity, IDisplayInfo
    {
        public EnumUnitConfigType UnitConfigType { get; set; }

        public int DisplayOrder { get; set; }

        public int DisplayNumber { get; set; }

        public double Percent { get; set; }

        public int OpenOrder { get; set; }

        public Guid UnitId { get; set; }

        public Unit? Unit { get; set; }

        public Guid OriginalId { get; set; }
    }
}