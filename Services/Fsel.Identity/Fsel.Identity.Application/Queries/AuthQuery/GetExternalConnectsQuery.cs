// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.AuthQuery
{
    using Common.ActionResults;
    using Core.Base;
    using Core.Base.Managers;
    using Domain.Entities;
    using Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.EntityFrameworkCore;

    public abstract class GetExternalConnectsQuery : IRequest<MethodResult<Dictionary<string, bool>>>
    {
    }

    public class GetExternalConnectsQueryHandler : IRequestHandler<GetExternalConnectsQuery, MethodResult<Dictionary<string, bool>>>
    {
        private readonly AuthContext _authContext;
        private readonly UserManager<User> _userManager;
        private readonly IUserRepository _userRepository;
        private readonly IAuthenticationSchemeProvider _authenticationSchemeProvider;

        public GetExternalConnectsQueryHandler(AuthContext authContext,
            UserManager<User> userManager,
            IUserRepository userRepository,
            IAuthenticationSchemeProvider authenticationSchemeProvider)
        {
            _authContext = authContext;
            _userManager = userManager;
            _userRepository = userRepository;
            _authenticationSchemeProvider = authenticationSchemeProvider;
        }

        public async Task<MethodResult<Dictionary<string, bool>>> Handle(GetExternalConnectsQuery request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.Queryable.FirstOrDefaultAsync(x => x.Id == _authContext.CurrentUserId, cancellationToken);
            if (user != null)
            {
                var externalLogins = await _userManager.GetLoginsAsync(user);
                var schemes = await _authenticationSchemeProvider.GetAllSchemesAsync();
                var externalProviders = schemes.Where(s => !string.IsNullOrEmpty(s.DisplayName))
                    .ToDictionary(x => x.Name, x => externalLogins.FirstOrDefault(l => l.LoginProvider == x.Name) != null);
                return new MethodResult<Dictionary<string, bool>>(externalProviders) { StatusCode = 200 };
            }

            return new MethodResult<Dictionary<string, bool>>() { StatusCode = 400 };
        }
    }
}
