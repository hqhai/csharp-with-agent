// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.UserQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetUsersByUserIdsQuery : IRequest<MethodResult<IList<UserModel>>>
    {
        public IList<Guid>? UserIds { get; set; }
    }

    public class GetUsersByUserIdsQueryHandler : IRequestHandler<GetUsersByUserIdsQuery, MethodResult<IList<UserModel>>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;

        public GetUsersByUserIdsQueryHandler(UserManager<User> userManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<UserModel>>> Handle(GetUsersByUserIdsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<UserModel>> methodResult = new MethodResult<IList<UserModel>>();
            if (request.UserIds == null || !request.UserIds.Any())
            {
                methodResult.Result = new List<UserModel>();
                return methodResult;
            }
            var users = await _userManager.Users.Where(x => request.UserIds.Contains(x.Id)).ToListAsync(cancellationToken);
            methodResult.Result = _mapper.Map<IList<UserModel>>(users);
            return methodResult;
        }
    }
}
