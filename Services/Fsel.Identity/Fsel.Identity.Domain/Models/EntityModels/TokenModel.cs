// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;

    public class TokenModel
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public string? FullName { get; set; }
        public string? Code { get; set; }
        public Guid? ClassId { get; set; }
        public string? ClassCode { get; set; }
        public DateTime? Expiration { get; set; }
        public IList<string>? Roles { get; set; }
        public bool? IsSurvey { get; set; } = true;
        public bool? IsOrder { get; set; } = true;
        public Guid? SchoolId { get; set; }
        public bool? IsPlacementTest { get; set; } = true;
        public EnumUserStatus? Status { get; set; }
    }
}
