// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.AuthCmd
{
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums;
    using Fsel.Common.Helpers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.Auths;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;

    public class ComfirmOTPCommand : ConfirmOTPCommandModel, IRequest<MethodResult<TokenModel>>
    {
    }

    public class ComfirmOTPCommandHandler : IRequestHandler<ComfirmOTPCommand, MethodResult<TokenModel>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly SignInManager<User> _signInManager;
        private readonly IHumanRepository _humanRepository;

        public ComfirmOTPCommandHandler(UserManager<User> userManager
            , IMediator mediator
            , IMapper mapper
            , SignInManager<User> signInManager
            , IHumanRepository humanRepository)
        {
            _userManager = userManager;
            _mediator = mediator;
            _mapper = mapper;
            _signInManager = signInManager;
            _humanRepository = humanRepository;
        }

        public async Task<MethodResult<TokenModel>> Handle(ComfirmOTPCommand request, CancellationToken cancellationToken)
        {
            MethodResult<TokenModel> methodResult = new MethodResult<TokenModel>();

            User user = new User();
            if (request?.Email != null)
            {
                user = await _userManager.Users.FirstOrDefaultAsync(e => e.Email == request.Email);
            }

            if (user == null)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddErrorMessage(
                    nameof(EnumAuthErrorCode.AU04V),
                    new[] { MethodHelper.GenerateErrorResult(nameof(request.Email), request?.Email) });
                return methodResult;
            }
            bool signInResult = false;

            if (request?.Email != null && request.Code != null)
            {
                signInResult = await _userManager.VerifyTwoFactorTokenAsync(user, "Email", request.Code);
            }

            if (!signInResult)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddErrorMessage(
                    nameof(EnumAuthErrorCode.AU15ER),
                    new[] { MethodHelper.GenerateErrorResult(nameof(request.Email), request?.Email) });
                return methodResult;
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            await _userManager.ConfirmEmailAsync(user, token);
            await _userManager.SetTwoFactorEnabledAsync(user, false);
            await _signInManager.SignInAsync(user, false);

            var roles = await _userManager.GetRolesAsync(user);
            Human human = _mapper.Map<Human>(user);
            human.UserId = user.Id;

            if (roles.Contains(EnumRoleRegister.Student.ToString()))
            {
                human.Student = new Student
                {
                    HumanId = human.Id,
                };
            }
            else if (roles.Contains(EnumRoleRegister.Parent.ToString()))
            {
                human.Parent = new Parent
                {
                    HumanId = human.Id,
                };
            }

            await _humanRepository.ExecuteTransactionAsync(async () =>
            {
                human = _humanRepository.Add(human);

                await _humanRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                return methodResult;
            });

            methodResult = await _mediator.Send(new GenerateTokenCommand { Id = user.Id }, cancellationToken).ConfigureAwait(false);
            return methodResult;
        }
    }
}
