namespace Fsel.Identity.Domain.Models.EntityModels
{
    public class OTPModel
    {
        public string? OTPEmail { get; set; }
        public bool? IsConfirmOTPEmail { get; set; }
        public string? OTPPhoneNumber { get; set; }
        public bool? IsConfirmOTPPhoneNumber { get; set; }
    }
}
