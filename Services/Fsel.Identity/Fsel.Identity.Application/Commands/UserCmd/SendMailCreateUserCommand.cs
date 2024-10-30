// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.UserCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Identity.Application.Services.SenderService;
    using Fsel.Identity.Infrastructure.ValueSettings;
    using Fsel.Shared.Enums;
    using MediatR;

    public class SendMailCreateUserCommand : IRequest<MethodResult<bool>>
    {
        public string? UserName { get; set; }
        public string? Password { get; set; }
    }

    public class SendMailCreateUserCommandHandler : IRequestHandler<SendMailCreateUserCommand, MethodResult<bool>>
    {
        private readonly AppSetting _appSetting;
        private readonly ISenderService _senderService;
        private const string Subject = "Thông tin tài khoản học";

        public SendMailCreateUserCommandHandler(AppSetting appSetting, ISenderService senderService)
        {
            _appSetting = appSetting;
            _senderService = senderService;
        }

        public async Task<MethodResult<bool>> Handle(SendMailCreateUserCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            if (string.IsNullOrEmpty(request.UserName) || string.IsNullOrEmpty(request.Password))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required));
                return methodResult;
            }

            await _senderService.SendEmailAsync(new SendEmailByTemplateCommandModel()
            {
                ToEmails = new[] { request.UserName },
                Template = EnumSenderTemplate.CreateUser,
                Subject = Subject,
                Params = new
                {
                    UserName = request.UserName,
                    Password = request.Password,
                    LinkLMS = _appSetting.ResourceContent?.LmsWebsiteUrl
                }
            });
            return methodResult;
        }
    }
}
