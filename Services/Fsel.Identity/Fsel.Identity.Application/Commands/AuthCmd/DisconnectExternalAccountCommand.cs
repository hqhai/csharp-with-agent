// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.AuthCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using MediatR;

    public class DisconnectExternalAccountCommand : IRequest<MethodResult<bool>>
    {
        public string Provider { get; set; }
    }

    public class DisconnectExternalAccountCommandHandler : IRequestHandler<DisconnectExternalAccountCommand, MethodResult<bool>>
    {
        private readonly UserManager<User> _userManager;
        private readonly AuthContext _authContext;
        private readonly IUserRepository _userRepository;

        public DisconnectExternalAccountCommandHandler(UserManager<User> userManager,
            AuthContext authContext,
            IUserRepository userRepository)
        {
            _userManager = userManager;
            _authContext = authContext;
            _userRepository = userRepository;
        }

        public async Task<MethodResult<bool>> Handle(DisconnectExternalAccountCommand request, CancellationToken cancellationToken)
        {
            var user = _userRepository.Queryable.FirstOrDefault(x => x.Id == _authContext.CurrentUserId);
            if (user != null)
            {
                var logins = await _userManager.GetLoginsAsync(user);

                var loginNeedToDisconnect = logins?.FirstOrDefault(x => x.LoginProvider == request.Provider);
                if (loginNeedToDisconnect != null)
                {
                    var result = await _userManager.RemoveLoginAsync(user, loginNeedToDisconnect.LoginProvider, loginNeedToDisconnect.ProviderKey);
                    return new MethodResult<bool>(result.Succeeded);
                }
            }

            return new MethodResult<bool>(false);
        }
    }
}
