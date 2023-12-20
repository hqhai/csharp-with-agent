// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    using System;
    using System.Collections.Generic;

    public class TokenNumber
    {
        public int? Number { get; set; }
    }

    public class TokenFocusTime
    {
        public IList<FocusTimeNumber>? FocusTimes { get; set; }
    }

    public class FocusTimeNumber : TokenNumber
    {
        public Guid FocusTimeId { get; set; }
    }

    public class TokenDailyCheckIn
    {
        public IList<DailyCheckInNumber>? DailyCheckIns { get; set; }
    }

    public class DailyCheckInNumber : TokenNumber
    {
        public int Level { get; set; }
    }
}
