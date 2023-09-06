// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.UserQuery
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Identity.Domain.Models.QueryModels.Users;
    using MediatR;
    using Microsoft.AspNetCore.Identity;

    public class GetUsersByRoleQuery : GetUsersByRoleQueryModel, IRequest<MethodResult<IList<UserModel>>>
    {
    }

    public class GetUsersByRoleQueryHandler : IRequestHandler<GetUsersByRoleQuery, MethodResult<IList<UserModel>>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;

        public GetUsersByRoleQueryHandler(UserManager<User> userManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<UserModel>>> Handle(GetUsersByRoleQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<UserModel>>();
            var users = await _userManager.GetUsersInRoleAsync(request.Role.ToString());
            methodResult.Result = _mapper.Map<IList<UserModel>>(users);
            return methodResult;
        }
    }
}
