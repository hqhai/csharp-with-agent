// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;

    public class SearchDetailReferralCodeModel
    {
        public Guid ReceiverId { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public DateTime CreatedDate { get; set; }
        public EnumUserReferralType Type { get; set; }
        public int Token { get; set; }
        public EnumFeatureUserReferral? UserReferral { get; set; }
    }
}
