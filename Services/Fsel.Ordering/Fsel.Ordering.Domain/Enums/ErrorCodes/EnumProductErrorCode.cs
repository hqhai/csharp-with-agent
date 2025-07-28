// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Enums.ErrorCodes
{
    public enum EnumProductErrorCode
    {
        ProductNotExist,
        NotPartOfTheEvent,
        ExchangeExpirationDate,
        OutOfQuantity,
        NotEnoughTokens,
        CodeAlreadyExists,
        NameAlreadyExists,
        WrongQuantity,
        WrongExpirationDate,
        EmptyPhoneNumber,
        PhoneNumberIsInvalid,
        GiftExchangeOff,
        TransactionInProgress
    }
}
