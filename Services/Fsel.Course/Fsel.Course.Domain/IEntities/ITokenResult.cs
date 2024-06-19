// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.IEntities
{
    public interface ITokenResult
    {
        public int? TokenFirstTime { get; set; }
        public int? TokenLastTime { get; set; }
    }
}
