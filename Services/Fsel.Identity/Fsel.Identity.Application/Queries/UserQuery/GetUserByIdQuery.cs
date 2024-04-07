// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.UserQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetUserByIdQuery : IRequest<MethodResult<UserModel>>
    {
        public Guid? Id { get; set; }
    }

    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, MethodResult<UserModel>>
    {
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;

        public GetUserByIdQueryHandler(IMapper mapper, UserManager<User> userManager)
        {
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<MethodResult<UserModel>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<UserModel> methodResult = new MethodResult<UserModel>();

            var user = await _userManager.Users.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
            if (user == null)
            {
                return methodResult;
            }
            methodResult.Result = _mapper.Map<UserModel>(user);
            return methodResult;
        }
    }
}
