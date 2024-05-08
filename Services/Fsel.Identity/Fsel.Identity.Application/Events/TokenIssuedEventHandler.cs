// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using IdentityServer4.Events;
    using IdentityServer4.Services;

    public class TokenIssuedEventHandler : IEventSink
    {
        private readonly AuthContext _authContext;

        public TokenIssuedEventHandler(AuthContext authContext)
        {
            _authContext = authContext;
        }

        public async Task PersistAsync(Event evt)
        {
            ArgumentNullException.ThrowIfNull(evt);

            if (evt.Id == EventIds.TokenIssuedSuccess)
            {
                var tokenIssuedEvent = evt as TokenIssuedSuccessEvent;
                // Xử lý sự kiện khi token được phát hành thành công ở đây
                // Ví dụ: Ghi log, thống kê, v.v.
                // evt.Context chứa thông tin liên quan đến token phát hành
            }
        }
    }
}
