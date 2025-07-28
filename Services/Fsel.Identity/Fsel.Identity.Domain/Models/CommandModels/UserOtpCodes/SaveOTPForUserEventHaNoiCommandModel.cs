// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.UserOtpCodes
{
    public class SaveOTPForUserEventHaNoiCommandModel
    {
        public EnumActionSaveOTPForEventHaNoi Action { get; set; }
        public int CountOTP { get; set; }
    }

    public enum EnumActionSaveOTPForEventHaNoi
    {
        Success,
        UpdateInFo,
        LMS
    }
}
