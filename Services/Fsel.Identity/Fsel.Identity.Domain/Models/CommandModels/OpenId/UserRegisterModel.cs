// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.OpenId
{
    using System.ComponentModel.DataAnnotations;
    using Fsel.Identity.Domain.Constants;

    public class UserRegisterModel
    {
        [RegularExpression("^(?:\\+|(?=\\d{10}))\\d{11,15}$", ErrorMessage = "i18n_number_phone_invalid")]
        [Required(ErrorMessage = "i18n_Phone_number_cannot_empty")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "i18n_First_name_cannot_be_empty")]
        [MaxLength(50, ErrorMessage = "i18n_limit_number_characters")]
        [RegularExpression(RegexSettings.FullNameValid, ErrorMessage = "i18n_First_name_is_not_valid")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "i18n_Last_name_cannot_be_empty")]
        [MaxLength(50, ErrorMessage = "i18n_limit_number_characters")]
        [RegularExpression(RegexSettings.FullNameValid, ErrorMessage = "i18n_Last_name_is_not_valid")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "i18n_Birth_day_cannot_be_empty")]
        public DateTime? Birthday { get; set; }

        [DataType(DataType.Password)]
        [RegularExpression(RegexSettings.PasswordValid, ErrorMessage = "i18n_Password_is_not_valid")]
        [Required(ErrorMessage = "i18n_Password_cannot_be_empty")]
        public string? Password { get; set; }

        public string? ReturnUrl { get; set; }

        public bool? IsShowVerifyOtp { get; set; }
    }
}
