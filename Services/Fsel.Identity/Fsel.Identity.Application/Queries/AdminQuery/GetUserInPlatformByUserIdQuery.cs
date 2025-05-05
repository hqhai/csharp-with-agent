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

        public GetUserInPlatformByUserIdQueryHandler(UserManager<User> userManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<MethodResult<UserModel>> Handle(GetUserInPlatformByUserIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<UserModel>();

            var user = await _userManager.Users
                    .Include(u => u.Human)
                    .Include(u => u.UserGroups)
                    .ThenInclude(ug => ug.Group)
                    .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken);

            if (user == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Id));
                return methodResult;
            }

            var userModel = _mapper.Map<UserModel>(user);
            userModel.UserGroupId = user.UserGroups?.FirstOrDefault()?.GroupId;
            userModel.UserGroupName = user.UserGroups?.FirstOrDefault()?.Group?.GroupName;
            methodResult.Result = userModel;

            // Nếu Human null thì không cần lấy thông tin của Người quản lý
            if (userModel.Human == null)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            // Lấy thông tin người quản lý
            var userManager = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Id == userModel.Human.ManageUserId, cancellationToken);

            if (userManager != null)
            {
                userModel.Human.ManageUserName = userManager.FullName;
            }

            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
