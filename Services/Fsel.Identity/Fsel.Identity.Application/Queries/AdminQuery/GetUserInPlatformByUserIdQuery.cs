// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.AdminQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetUserInPlatformByUserIdQuery : BaseCommandModel, IRequest<MethodResult<UserModel>>
    {
    }

    public class GetUserInPlatformByUserIdQueryHandler : IRequestHandler<GetUserInPlatformByUserIdQuery, MethodResult<UserModel>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly RoleManager<Role> _roleManager;


        public GetUserInPlatformByUserIdQueryHandler(UserManager<User> userManager, IMapper mapper, IUserRoleRepository userRoleRepository, RoleManager<Role> roleManager)
        {
            _userManager = userManager;
            _mapper = mapper;
            _userRoleRepository = userRoleRepository;
            _roleManager = roleManager;
        }

        public async Task<MethodResult<UserModel>> Handle(GetUserInPlatformByUserIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<UserModel>();

            var user = await _userManager.Users
                    .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken);

            var userRole = await _userRoleRepository.GetRoleIdsAndNamesByUserIdAsync(request.Id, cancellationToken);


            if (user == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Id));
                return methodResult;
            }

            var userModel = _mapper.Map<UserModel>(user);
            userModel.UserGroupId = userRole.RoleId;
            userModel.UserGroupName = userRole.RoleName;
            methodResult.Result = userModel;

            // Lấy thông tin người quản lý
            var userManager = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Id == userModel.ManageUserId, cancellationToken);

            if (userManager != null)
            {
                userModel.ManageUserName = userManager.FullName;
            }

            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
