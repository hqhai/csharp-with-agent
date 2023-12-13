// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.IEntities
{
    public interface ITokenResult
    {
        public int TokenDone { get; set; }
        public int TokenHighestStreak { get; set; }
        public int TokenSuperFire { get; set; }
    }
}
