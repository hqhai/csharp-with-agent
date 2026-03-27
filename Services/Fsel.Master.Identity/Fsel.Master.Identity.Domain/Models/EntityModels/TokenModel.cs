// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Master.Identity.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;

    public class TokenModel
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public string? FullName { get; set; }
        public string? Code { get; set; }
        public DateTime? Expiration { get; set; }
        public IList<string>? Roles { get; set; }
        public EnumUserStatus? Status { get; set; }
    }
}
