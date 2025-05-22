// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Enums.ErrorCodes
{
    public enum EnumVoucherErrorCode
    {
        VoucherStartTimeMustSoonerThanEndTime,
        VoucherIsUsed,
        VoucherNotExist,
        VoucherNotActive,
        VoucherHasExpired,
        VoucherOutOfQuantity,
        NotSubjectToUse,
        VoucherDoesNotApplyToThisPackage,
        ValueGreaterThan100,
        CodeAlreadyExists,
        CodePrefixAlreadyExists,
        CodeInValidFormat,
        WrongEventApplied,
        TheNumberOfUsesHasExpired
    }
}
