// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.OpenId
{
    using System.ComponentModel.DataAnnotations;
    using Fsel.Core.Base.Interfaces;

    public class ForgotModel : IRequestBodyTenantAware
    {
        [Required(ErrorMessage = "i18nFieldRequired")]
        public string? Identity { get; set; }

        public string? ReturnUrl { get; set; }

        public string? UserName { get; set; }

        public Guid? UserId { get; set; }
    }
}
