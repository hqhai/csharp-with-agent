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

    public class GetExternalConnectsQuery : IRequest<MethodResult<IEnumerable<string>>>
    {
    }

    public class GetExternalConnectsQueryHandler : IRequestHandler<GetExternalConnectsQuery, MethodResult<IEnumerable<string>>>
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

        public async Task<MethodResult<IEnumerable<string>>> Handle(GetExternalConnectsQuery request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.Queryable.FirstOrDefaultAsync(x => x.Id == _authContext.CurrentUserId, cancellationToken);
            if (user != null)
            {
                var externalLogins = await _userManager.GetLoginsAsync(user);
                var schemes = await _authenticationSchemeProvider.GetAllSchemesAsync();
                return new MethodResult<IEnumerable<string>>(externalLogins.Select(x => x.ProviderDisplayName)) { StatusCode = 200 };
            }

            return new MethodResult<IEnumerable<string>>() { StatusCode = 400 };
        }
    }
}
