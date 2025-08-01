// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Domain.Entities;
    using IdentityServer4.Events;
    using IdentityServer4.Services;
    using Microsoft.AspNetCore.Http;

    public class TokenIssuedEventHandler : IEventSink
    {
        private readonly AuthContext _authContext;
        private UserManager<User> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITenantProvider _tenantProvider;

        public TokenIssuedEventHandler(UserManager<User> userManager, AuthContext authContext, IHttpContextAccessor httpContextAccessor, ITenantProvider tenantProvider)
        {
            _userManager = userManager;
            _authContext = authContext;
            _httpContextAccessor = httpContextAccessor;
            _tenantProvider = tenantProvider;
        }


        public async Task PersistAsync(Event evt)
        {
            ArgumentNullException.ThrowIfNull(evt);
            ArgumentNullException.ThrowIfNull(_httpContextAccessor.HttpContext);

            if (evt.Id == EventIds.TokenIssuedSuccess)
            {
                var tokenIssuedEvent = evt as TokenIssuedSuccessEvent;

                _userManager = await _tenantProvider.CreateUserManagerAsync<User>(userId: tokenIssuedEvent?.SubjectId.Parse<Guid>()) ?? _userManager;
                var user = await _userManager.FindByIdAsync(tokenIssuedEvent?.SubjectId ?? string.Empty);
                // Xử lý sự kiện khi token được phát hành thành công ở đây
                // Ví dụ: Ghi log, thống kê, v.v.
                // evt.Context chứa thông tin liên quan đến token phát hành
            }
        }
    }
}
