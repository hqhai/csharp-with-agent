// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    public class ConfirmOtpModel : TokenModel
    {
        public Guid? UserId { get; set; }
    }
}
