// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Common.Enums.ErrorCodes;
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
            private readonly IMapper _mapper;
            
            public Handler(
                IUserGroupRepository userGroupRepository,
                IUserGroupMemberShipRepository userGroupMemberShipRepository,
                IMapper mapper)
            {
                _userGroupRepository = userGroupRepository;
                _userGroupMemberShipRepository = userGroupMemberShipRepository;
                _mapper = mapper;
            }
            
            public async Task<MethodResult<UserGroupModel>> Handle(GetUserGroupByIdQuery request, CancellationToken cancellationToken)
            {
                var methodResult = new MethodResult<UserGroupModel>();
                
                // Get the user group
                var userGroup = await _userGroupRepository.GetByIdAsync(request.Id);
                if (userGroup == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(UserGroup));
                    return methodResult;
                }
                
                // Map to model
                var userGroupModel = _mapper.Map<UserGroupModel>(userGroup);
                
                // Get members of the group
                var members = await _userGroupMemberShipRepository.Queryable
                    .Where(x => x.GroupId == request.Id && x.IsActive)
                    .Include(x => x.User)
                    .ToListAsync(cancellationToken);
                
                userGroupModel.Members = _mapper.Map<List<UserGroupMemberShipModel>>(members);
                
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = userGroupModel;
                
                return methodResult;
            }
        }
    }
} 
