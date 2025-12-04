// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.UserQuery
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Identity.Domain.Models.QueryModels.Users;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetUsersByIdsQuery : GetUsersByIdsQueryModel, IRequest<MethodResult<IList<UserModel>>>
    {
    }

    public class GetUsersByIdsQueryHandler : IRequestHandler<GetUsersByIdsQuery, MethodResult<IList<UserModel>>>
    {
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;

        public GetUsersByIdsQueryHandler(IMapper mapper, UserManager<User> userManager)
        {
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<MethodResult<IList<UserModel>>> Handle(GetUsersByIdsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<UserModel>> methodResult = new MethodResult<IList<UserModel>>();

            if (request.UserIds == null || !request.UserIds.Any())
            {
                methodResult.Result = new List<UserModel>();
                return methodResult;
            }
            var users = await _userManager.Users.Where(p => request.UserIds.Contains(p.Id)).ToListAsync(cancellationToken);
            methodResult.Result = _mapper.Map<IList<UserModel>>(users);
            return methodResult;
        }
    }
}
