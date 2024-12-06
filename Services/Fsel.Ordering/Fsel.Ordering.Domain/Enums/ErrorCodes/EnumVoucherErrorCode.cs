// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Enums.ErrorCodes
{
    public enum EnumVoucherErrorCode
    {
        /// <summary>
        /// Voucher Start Time Must be Sooner Than the End Time
        /// </summary>
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
        WrongEventApplied
    }
}
