// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    public class TokenModel
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public string? FullName { get; set; }
        public Guid? ClassId { get; set; }
        public DateTime? Expiration { get; set; }
        public IList<string>? Roles { get; set; }
        public bool? IsSurvey { get; set; } = true;
    }
}
