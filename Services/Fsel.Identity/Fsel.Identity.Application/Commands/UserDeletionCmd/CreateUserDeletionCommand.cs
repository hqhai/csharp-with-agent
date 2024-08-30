// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.UserDeletionCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.UserDeletions;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Hosting;

    public class CreateUserDeletionCommand : CreateUserDeletionCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class CreateUserDeletionCommandHandler : IRequestHandler<CreateUserDeletionCommand, MethodResult<bool>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IHostEnvironment _environment;
        private readonly IUserDeletionRepository _userDeletionRepository;
        private readonly AuthContext _authContext;
        private readonly IMapper _mapper;

        public CreateUserDeletionCommandHandler(UserManager<User> userManager, IHostEnvironment environment, IUserDeletionRepository userDeletionRepository, AuthContext authContext, IMapper mapper)
        {
            _userManager = userManager;
            _environment = environment;
            _userDeletionRepository = userDeletionRepository;
            _authContext = authContext;
            _mapper = mapper;
        }

        public async Task<MethodResult<bool>> Handle(CreateUserDeletionCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();
            if (string.IsNullOrEmpty(request.Password))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Password));
                return methodResult;
            }
            var user = await _userManager.FindByIdAsync(_authContext.CurrentUserId.ToString());
            if (user == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(user));
                return methodResult;
            }

            if (!await _userManager.CheckPasswordAsync(user, request.Password))
            {
                methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.PasswordIncorrect), nameof(user));
                return methodResult;
            }

            var userDeletion = await _userDeletionRepository.Queryable.FirstOrDefaultAsync(x => x.UserId == _authContext.CurrentUserId && x.Status == EnumUserDeletionStatus.New, cancellationToken);
            if (userDeletion != null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(userDeletion));
                return methodResult;
            }

            userDeletion = _mapper.Map<UserDeletion>(request);
            userDeletion.PhoneNumber = user.PhoneNumber;
            userDeletion.Email = user.Email;
            userDeletion.FullName = user.FullName;
            userDeletion.UserId = user.Id;
            if (_environment.IsDevelopment() || _environment.IsEnvironment(Settings.Environments.Testing) || _environment.IsStaging())
            {
                userDeletion.DeletionDate = DateTime.UtcNow.AddMinutes(10);
            }

            if (!userDeletion.IsValid())
            {
                methodResult.AddErrorBadRequest(userDeletion.ErrorMessages);
                return methodResult;
            }

            _userDeletionRepository.Add(userDeletion);
            await _userDeletionRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

            methodResult.Result = true;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
