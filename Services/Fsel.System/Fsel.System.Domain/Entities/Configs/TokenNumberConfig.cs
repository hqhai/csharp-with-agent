// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities.Configs
{
    public class TokenNumber
    {
        public int? Number { get; set; }
    }

    public class TokenFocusTimeNumber
    {
        public Guid FocusTimeId { get; set; }
        public int? Number { get; set; }
    }

    public class TokenDailyCheckinNumber
    {
        public int Level { get; set; }
        public int? Number { get; set; }
    }
}
