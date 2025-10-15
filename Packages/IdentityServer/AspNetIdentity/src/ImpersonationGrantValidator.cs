// Copyright (c) Atlantic. All rights reserved.

namespace IdentityServer4.AspNetIdentity
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Core.Entities;
    using IdentityServer4.Models;
    using IdentityServer4.Validation;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using static IdentityModel.OidcConstants;

    /// <summary>
    /// IResourceOwnerPasswordValidator that integrates with ASP.NET Identity.
    /// </summary>
    /// <typeparam name="TUser">The type of the user.</typeparam>
    /// <seealso cref="IExtensionGrantValidator" />
    public class ImpersonationGrantValidator<TUser> : IExtensionGrantValidator
        where TUser : UserEntity
    {
        private UserManager<TUser> _userManager;
        private readonly SignInManager<TUser> _signInManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ImpersonationGrantValidator<TUser>> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImpersonationGrantValidator{TUser}"/> class.
        /// </summary>
        /// <param name="userManager">The user manager.</param>
        /// <param name="signInManager">The sign in manager.</param>
        /// <param name="logger">The logger.</param>
        public ImpersonationGrantValidator(UserManager<TUser> userManager, SignInManager<TUser> signInManager, IServiceProvider serviceProvider, ILogger<ImpersonationGrantValidator<TUser>> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        /// <summary>
        /// GrantType
        /// </summary>
        public string GrantType => Models.GrantType.Impersonation;

        /// <summary>
        /// Validates the resource owner password credential
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public virtual async Task ValidateAsync(ExtensionGrantValidationContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            var userName = context.Request.Raw.Get(nameof(ExtensionGrantValidationContext.Request.UserName));

            var tenantProvider = _serviceProvider.GetService<ITenantProvider>();
            _userManager = tenantProvider != null ? await tenantProvider.CreateUserManagerAsync<TUser>(userName ?? string.Empty) ?? _userManager : _userManager;
            var user = await _userManager.FindByNameAsync(userName ?? string.Empty);
            if (user != null)
            {
                _logger.LogInformation("Credentials validated for username: {username}", userName);

                var sub = await _userManager.GetUserIdAsync(user);
                context.Result = new GrantValidationResult(sub, AuthenticationMethods.Password);
                return;
            }
            else
            {
                _logger.LogInformation("No user found matching username: {username}", userName);
            }

            context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant);
        }
    }
}
