namespace Fsel.Identity.Domain.Models.EntityModels
{
    using System.ComponentModel;
    using Fsel.Core.Base.BaseModels;

    public class UserReferralForCRMModel : BaseModel
    {
        public Guid StudentId { get; set; }
        public Guid SenderId { get; set; }
        public Guid ReceiveId { get; set; }
        public string? StudentCode { get; set; }
        public string? Name { get; set; }
        public string? ReferralCode { get; set; }
        public string? Package { get; set; }
        public ReferralHistoryStatus? Status { get; set; }
    }

    public enum ReferralHistoryStatus
    {
        [Description("Student not payment.")]
        NotPayment = 0,

        [Description("Student payment.")]
        Payment = 1
    }
}
