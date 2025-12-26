// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.IEntities
{
    public interface IDisplayInfo
    {
        public int DisplayOrder { get; set; }
        public int DisplayNumber { get; set; }
        public double Percent { get; set; }
        public int OpenOrder { get; set; }
    }
}
