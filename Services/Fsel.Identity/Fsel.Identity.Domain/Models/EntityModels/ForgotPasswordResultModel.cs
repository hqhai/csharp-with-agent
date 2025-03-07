// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    public class ForgotPasswordResultModel
    {
        public bool IsSuccess { get; set; }

        public int CountOTP { get; set; }
    }
}
