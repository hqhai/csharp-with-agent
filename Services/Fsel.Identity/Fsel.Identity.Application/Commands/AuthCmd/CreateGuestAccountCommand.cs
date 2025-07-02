// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.AuthCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using System.Transactions;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.Auths;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using UserManager = Core.Base.Managers.UserManager<Domain.Entities.User>;

    public class CreateGuestAccountCommand : CreateGuestAccountCommandModel, IRequest<MethodResult<TokenModel>>
    {
    }

    public class CreateGuestAccountCommandHandler : IRequestHandler<CreateGuestAccountCommand, MethodResult<TokenModel>>
    {
        private readonly UserManager _userManager;
        private readonly IPlatformRepository _platformRepository;
        private readonly SignInManager<User> _signInManager;
        private readonly IMediator _mediator;
        private const string DefaultPassword = "Hello.123";

        public CreateGuestAccountCommandHandler(UserManager userManager, IPlatformRepository platformRepository, SignInManager<User> signInManager, IMediator mediator)
        {
            _userManager = userManager;
            _platformRepository = platformRepository;
            _signInManager = signInManager;
            _mediator = mediator;
        }

        public async Task<MethodResult<TokenModel>> Handle(CreateGuestAccountCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<TokenModel> methodResult = new MethodResult<TokenModel>();

            //using (var scope = new TransactionScope(TransactionScopeOption.Required,
            //    new TransactionOptions
            //    {
            //        IsolationLevel = IsolationLevel.ReadCommitted
            //    },
            //    TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    #region User initialization

                    User? user = new User();
                    var countUser = await _userManager.Users.CountAsync(cancellationToken);
                    while (true)
                    {
                        user.UserName = EnumRole.Guest.ToString() + (countUser);
                        var existUserName = await _userManager.Users.FirstOrDefaultAsync(p => p.UserName == user.UserName, cancellationToken);
                        if (existUserName == null)
                        {
                            break;
                        }
                        countUser++;
                    }
                    user.Email = user.UserName + "@gmail.com";
                    user.FirstName = user.UserName;
                    user.Code = user.Email;
                    user.EmailConfirmed = true;

                    #endregion User initialization

                    user.Student = new Student();

                    #region Add Platform to User

                    var platform = await _platformRepository.GetPlatformAsync(request.PlatformCode, cancellationToken);
                    if (platform != null)
                    {
                        user.UserPlatforms.Add(new UserPlatform
                        {
                            PlatformId = platform.Id
                        });
                    }

                    #endregion Add Platform to User

                    #region Create user and add role to user

                    var result = await _userManager.CreateAsync(user, DefaultPassword ?? string.Empty);
                    if (!result.Succeeded)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.UserFailToCreate));
                        return methodResult;
                    }
                    await _userManager.AddToRoleAsync(user, EnumRole.Guest.ToString());

                    #endregion Create user and add role to user

                    #region SignIn

                    var signInresult = await _signInManager.PasswordSignInAsync(user, DefaultPassword, false, false);
                    if (!result.Succeeded)
                    {
                        methodResult.AddError(
                            StatusCodes.Status401Unauthorized, nameof(EnumAuthUserErrorCode.UserNameAndPasswordIncorrect), new Error(nameof(user.UserName), user.UserName), new Error(nameof(DefaultPassword), DefaultPassword));
                        return methodResult;
                    }
                    var generateToken = await _mediator.Send(new GenerateTokenCommand { Id = user.Id }, cancellationToken).ConfigureAwait(false);
                    //scope.Complete();

                    #endregion SignIn

                    methodResult = generateToken;
                    return methodResult;
                }
                catch
                {
                    methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.SignUpFail));
                    //scope.Dispose();
                }
                return methodResult;
            }
        }
    }
}
