// Copyright (c) Atlantic. All rights reserved.

using System.Linq.Dynamic.Core;
using Fsel.Common.ActionResults;
using Fsel.Core.Base.Managers;
using Fsel.Identity.Application.Commands.UserDeletionCmd;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.Enums;
using Fsel.Identity.Domain.Enums.ErrorCodes;
using Fsel.Identity.Domain.IRepositories;
using Fsel.Identity.Domain.Models.CommandModels.Auths;
using Fsel.Identity.Domain.Models.EntityModels;
using Fsel.Shared.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Identity.Application.Commands.AuthCmd
{
    public class LoginCommand : LoginCommandModel, IRequest<MethodResult<TokenModel>>
    {
    }

    public class LoginCommandHandler : IRequestHandler<LoginCommand, MethodResult<TokenModel>>
    {
        private readonly UserManager<User> _userManager;
        private readonly Microsoft.AspNetCore.Identity.SignInManager<User> _signInManager;
        private readonly IMediator _mediator;
        private readonly IPlatformRepository _platformRepository;
        private readonly IStudentCompetitionEventsRepository _studentCompetitionEventsRepository;
        private readonly IHumanRepository _humanRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly ICompetitionEventsRepository _competitionEventsRepository;

        public LoginCommandHandler(UserManager<User> userManager,
            Microsoft.AspNetCore.Identity.SignInManager<User> signInManager,
            IMediator mediator,
            IPlatformRepository platformRepository,
            IStudentCompetitionEventsRepository studentCompetitionEventsRepository,
            IHumanRepository humanRepository,
            IStudentRepository studentRepository,
            ICompetitionEventsRepository competitionEventsRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _mediator = mediator;
            _platformRepository = platformRepository;
            _studentCompetitionEventsRepository = studentCompetitionEventsRepository;
            _humanRepository = humanRepository;
            _studentRepository = studentRepository;
            _competitionEventsRepository = competitionEventsRepository;
        }

        public async Task<MethodResult<TokenModel>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            request.Username = request.Username?.Trim();
            MethodResult<TokenModel> methodResult = new MethodResult<TokenModel>();

            if ((request.Username == null || string.IsNullOrWhiteSpace(request.Username)) || request.Password == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.UserNameAndPasswordNotEmpty), new Error(nameof(request.Username)), new Error(nameof(request.Password)));
                return methodResult;
            }

            var user = await _userManager.FindByNameAsync(request.Username);
            if (user == null || user.IsDeleted)
            {
                methodResult.AddError(StatusCodes.Status401Unauthorized, nameof(EnumAuthUserErrorCode.UserNameAndPasswordIncorrect), new Error(nameof(request.Username), request.Username), new Error(nameof(request.Password), request.Password));
                return methodResult;
            }

            var platformCodes = await _platformRepository.Queryable.Include(x => x.UserPlatforms).Where(x => x.UserPlatforms.Select(n => n.UserId).Contains(user.Id)).Select(x => x.Code).ToListAsync(cancellationToken);
            if (platformCodes != null && platformCodes.Count > 0 && !platformCodes.Contains(request.PlatformCode))
            {
                methodResult.AddError(StatusCodes.Status401Unauthorized, nameof(EnumAuthUserErrorCode.UserIsNotOnAnyPlatform), new Error(nameof(request.Username), request.Username), new Error(nameof(request.Password), request.Password));
                return methodResult;
            }

            if (user.Status.HasValue && user.Status == EnumUserStatus.Inactive)
            {
                methodResult.AddError(StatusCodes.Status401Unauthorized, nameof(EnumAuthUserErrorCode.AccountHasBeenLocked), new Error(nameof(request.Username), request.Username));
                return methodResult;
            }
            //else if (user.Status.HasValue && user.Status == EnumUserStatus.Disable)
            //{
            //    methodResult.AddError(StatusCodes.Status401Unauthorized, nameof(EnumAuthUserErrorCode.AccountHasBeenCutOff), new Error(nameof(request.Username), request.Username));
            //    return methodResult;
            //}

            var isCheckPassword = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!isCheckPassword)
            {
                methodResult.AddError(
                                      StatusCodes.Status401Unauthorized, nameof(EnumAuthUserErrorCode.UserNameAndPasswordIncorrect), new Error(nameof(request.Username), request.Username), new Error(nameof(request.Password), request.Password));
                return methodResult;
            }

            var competitionEvent = await (from baseQ in _humanRepository.Queryable
                                          join s in _studentRepository.Queryable on baseQ.Id equals s.HumanId
                                          join sce in _studentCompetitionEventsRepository.Queryable on s.Id equals sce.StudentId
                                          join ce in _competitionEventsRepository.Queryable on sce.CompetitionEventId equals ce.Id
                                          where baseQ.UserId == user.Id
                                          select ce).FirstOrDefaultAsync(cancellationToken);

            var isByPassEmailComfirm = competitionEvent?.EventContent?.IsByPassEmailComfirm ?? default;
            if (user.EmailConfirmed || !isByPassEmailComfirm)
            {
                var result = await _signInManager.PasswordSignInAsync(user, request.Password, false, false);
                if (!result.Succeeded)
                {
                    methodResult.AddError(
                        StatusCodes.Status401Unauthorized, nameof(EnumAuthUserErrorCode.UserNameAndPasswordIncorrect), new Error(nameof(request.Username), request.Username), new Error(nameof(request.Password), request.Password));
                    return methodResult;
                }
            }

            await _mediator.Send(new UpdateStatusUserDeletionCommand { UserId = user.Id, Status = EnumUserDeletionStatus.Cancel }, cancellationToken).ConfigureAwait(false);
            var generateToken = await _mediator.Send(new GenerateTokenCommand { Id = user.Id }, cancellationToken).ConfigureAwait(false);

            if (generateToken.Result != null)
            {
                generateToken.Result.Status = user.Status;
            }

            methodResult = generateToken;
            return methodResult;
        }
    }
}
