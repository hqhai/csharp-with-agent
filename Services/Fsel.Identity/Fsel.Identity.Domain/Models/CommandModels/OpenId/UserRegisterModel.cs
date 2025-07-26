// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.OpenId
{
    using System.ComponentModel.DataAnnotations;
    using Fsel.Identity.Domain.Constants;
    using Fsel.Shared.Enums;

    public class UserRegisterModel
    {
        //[RegularExpression(RegexSettings.EmailValid, ErrorMessage = "i18n_Email_is_not_valid")]
        //[Required(ErrorMessage = "i18n_Email_cannot_be_empty")]
        //[MaxLength(254, ErrorMessage = "i18n_limit_number_characters")]
        //public string? Email { get; set; }

        [RegularExpression(Common.Helpers.RegexHelper.PhoneNumberValid, ErrorMessage = "i18n_number_phone_invalid")]
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

        public EnumGender? Gender { get; set; }

        public int? DayBirthday { get; set; }

        public int? MonthBirthday { get; set; }

        public int? YearBirthday { get; set; }

        [RegularExpression(nameof(BirthdayStr), ErrorMessage = "i18n_Invalid_birthday")]
        public string? BirthdayStr
        {
            get
            {
                if (DayBirthday.HasValue && MonthBirthday.HasValue && YearBirthday.HasValue && Birthday.HasValue)
                {
                    return nameof(BirthdayStr);
                }
                else if (DayBirthday.HasValue && MonthBirthday.HasValue && YearBirthday.HasValue && !Birthday.HasValue)
                {
                    return nameof(Birthday);
                }
                return null;
            }
        }

        [Required(ErrorMessage = "i18n_Birth_day_cannot_be_empty")]
        public DateTime? Birthday
        {
            get
            {
                try
                {
                    return DayBirthday.HasValue && MonthBirthday.HasValue && YearBirthday.HasValue ? new DateTime(YearBirthday.Value, MonthBirthday.Value, DayBirthday.Value) : null;
                }
                catch
                {
                    return null;
                }
            }
        }

        [DataType(DataType.Password)]
        [RegularExpression(RegexSettings.PasswordValid, ErrorMessage = "i18n_Password_is_not_valid")]
        [Required(ErrorMessage = "i18n_Password_cannot_be_empty")]
        public string? Password { get; set; }

        //[DataType(DataType.Password)]
        //[Display(Name = "Confirm Password")]
        //[Compare(nameof(Password), ErrorMessage = "Password and confirmation password not match.")]
        //[Required(ErrorMessage = "Password confirmation cannot be empty.")]
        //public string? ConfirmPassword { get; set; }

        public string? ReferralCode { get; set; }

        public string? ReturnUrl { get; set; }

        public bool? IsShowVerifyOtp { get; set; }
    }
}
