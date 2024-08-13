// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Quickstarts
{
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Attributes;
    using Fsel.Identity.Domain.Constants;
    using Fsel.Shared.Enums;

    public class UserRegisterModel
    {
        [EmailValid(ErrorMessage = "Email is not valid.")]
        [Required(ErrorMessage = "Email cannot be empty.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "First name cannot be empty.")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "Last name cannot be empty.")]
        public string? LastName { get; set; }

        public EnumGender? Gender { get; set; }

        public int? DayBirthday { get; set; }

        public int? MonthBirthday { get; set; }

        public int? YearBirthday { get; set; }

        [Required(ErrorMessage = "Birth day cannot be empty.")]
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
        [RegexValid(ErrorMessage = "Password is not valid.", Regex = RegexSettings.Password)]
        [Required(ErrorMessage = "Password cannot be empty.")]
        public string? Password { get; set; }

        //[DataType(DataType.Password)]
        //[Display(Name = "Confirm Password")]
        //[Compare(nameof(Password), ErrorMessage = "Password and confirmation password not match.")]
        //[Required(ErrorMessage = "Password confirmation cannot be empty.")]
        //public string? ConfirmPassword { get; set; }

        public string? ReferralCode { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
