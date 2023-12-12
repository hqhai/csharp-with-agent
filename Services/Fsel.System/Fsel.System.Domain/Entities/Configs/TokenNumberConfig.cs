// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities.Configs
{
    public class TokenNumber
    {
        public int? Number { get; set; }
    }

    public class TokenForcusTime
    {
        public IList<FocusTimeNumber>? FocusTimes { get; set; }
    }

    public class FocusTimeNumber : TokenNumber
    {
        public Guid FocusTimeId { get; set; }
    }

    public class TokenDailyCheckin
    {
        public IList<DailyCheckinNumber>? DailyCheckins { get; set; }
    }


    public class DailyCheckinNumber : TokenNumber
    {
        public int Level { get; set; }
    }
}
