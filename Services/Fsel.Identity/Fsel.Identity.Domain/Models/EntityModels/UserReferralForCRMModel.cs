namespace Fsel.Identity.Domain.Models.EntityModels
{
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
        public string? Status { get; set; }
    }
}
