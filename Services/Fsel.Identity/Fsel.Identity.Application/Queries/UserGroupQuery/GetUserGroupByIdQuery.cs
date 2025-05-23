// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Core.Base.Managers;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.IRepositories;
using Fsel.Identity.Domain.Models.EntityModels;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Identity.Application.Queries.UserGroupQuery
{
    public class GetUserGroupByIdQuery : IRequest<MethodResult<UserGroupModel>>
    {
        public Guid Id { get; set; }

        public class Handler : IRequestHandler<GetUserGroupByIdQuery, MethodResult<UserGroupModel>>
        {
            private readonly IUserGroupRepository _userGroupRepository;
            private readonly IUserGroupMemberShipRepository _userGroupMemberShipRepository;
            private readonly IUserRoleRepository _userRoleRepository;
            private readonly IMapper _mapper;
            private readonly RoleManager<Role> _roleManager;

            public Handler(
                IUserGroupRepository userGroupRepository,
                IUserGroupMemberShipRepository userGroupMemberShipRepository,
                IMapper mapper,
                RoleManager<Role> roleManager,
                IUserRoleRepository userRoleRepository)
            {
                _userGroupRepository = userGroupRepository;
                _userGroupMemberShipRepository = userGroupMemberShipRepository;
                _mapper = mapper;
                _roleManager = roleManager;
                _userRoleRepository = userRoleRepository;
            }

            public async Task<MethodResult<UserGroupModel>> Handle(GetUserGroupByIdQuery request, CancellationToken cancellationToken)
            {
                var methodResult = new MethodResult<UserGroupModel>();

                // Get the user group
                var userGroup = await _roleManager.FindByIdAsync(request.Id.ToString());
                if (userGroup == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(UserGroup));
                    return methodResult;
                }

                // Map to model
                var userGroupModel = _mapper.Map<UserGroupModel>(userGroup);


                var memberQuery = _userRoleRepository.GetQuery();
                // Get members of the group
                var members = await memberQuery
                    .Where(x => x.RoleId == request.Id && x.IsActive)
                    .ToListAsync(cancellationToken);

                userGroupModel.Members = _mapper.Map<List<UserGroupMemberShipModel>>(members);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = userGroupModel;

                return methodResult;
            }
        }
    }
}
