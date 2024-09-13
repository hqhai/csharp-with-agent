// Copyright (c) Atlantic. All rights reserved.

using Microsoft.EntityFrameworkCore;

namespace Fsel.Identity.Application.Commands.StudentCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Models.CommandModels.Students;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class UpdateProfileStudentCommand : UpdateProfileStudentCommandModel, IRequest<MethodResult<UserModel>>
    {
    }

    public class UpdateProfileStudentCommandHandle : IRequestHandler<UpdateProfileStudentCommand, MethodResult<UserModel>>
    {
        private readonly UserManager<User> _userManager;
        private readonly AuthContext _authContext;
        private readonly IMapper _mapper;

        public UpdateProfileStudentCommandHandle(UserManager<User> userManager, AuthContext authContext, IMapper mapper)
        {
            _userManager = userManager;
            _authContext = authContext;
            _mapper = mapper;
        }

        public async Task<MethodResult<UserModel>> Handle(UpdateProfileStudentCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<UserModel>();

            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == _authContext.CurrentUserId, cancellationToken);
            if (user == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(user));
                return methodResult;
            }
            if (!string.IsNullOrEmpty(request.PhoneNumber) && !request.PhoneNumber.TryParse())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(request.PhoneNumber));
                return methodResult;
            }
            _mapper.Map(request, user);
            if (!user.IsValid())
            {
                methodResult.AddErrorBadRequest(user.ErrorMessages);
                return methodResult;
            }
            await _userManager.UpdateAsync(user);

            methodResult.Result = _mapper.Map<UserModel>(user);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
