// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Enums.ErrorCodes
{
    public enum EnumOTPCodeErrorCode
    {
        UserDoesNotExist,
        AttemptsExhausted,
        PendingVerification,
        NotInEventHN,
        OTPNotSentYet,
        WrongOTP,
        SentWithin30Seconds
    }
}
