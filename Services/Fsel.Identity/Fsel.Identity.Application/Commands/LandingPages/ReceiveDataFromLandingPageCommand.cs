// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.LandingPages
{
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Application.Services.SystemService;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Models.CommandModels.LandingPages;
    using Fsel.Identity.Infrastructure.ValueSettings;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Fsel.Identity.Application.Services.SenderService;

    public class ReceiveDataFromLandingPageCommand : ReceiveDataFromLandingPageCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class ReceiveDataFromLandingPageCommandHandler : IRequestHandler<ReceiveDataFromLandingPageCommand, MethodResult<bool>>
    {
        private readonly UserManager<User> _userManager;
        private readonly ISenderService _senderService;
        private readonly ISystemService _systemService;
        private readonly AppSetting _appSetting;
        private const string InstructUserNotExist = "Để tham gia vào chương trình này, quý khách truy cập theo đường dẫn dưới đây để xác nhận tạo tài khoản.";
        private const string InstructUserAlreadyExist = "Hệ thống nhận thấy email của bạn đã được đăng kí. Vui lòng truy cập đường dẫn để đăng nhập";

        public ReceiveDataFromLandingPageCommandHandler(UserManager<User> userManager, ISenderService senderService, ISystemService systemService, AppSetting appSetting)
        {
            _userManager = userManager;
            _senderService = senderService;
            _systemService = systemService;
            _appSetting = appSetting;
        }

        public async Task<MethodResult<bool>> Handle(ReceiveDataFromLandingPageCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            if (string.IsNullOrEmpty(request.FirstName) || string.IsNullOrEmpty(request.LastName) || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.LastName) || string.IsNullOrEmpty(request.PhoneNumber))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            await _systemService.AddContactInfoToGoogleSheet(request);

            var birthday = request.Birthday.ToString("dd-MM-yyyy", CultureInfo.CurrentCulture);
            var model = new ContactInfo
            {
                FullName = request.FirstName + " " + request.LastName,
                BirthDay = birthday,
                PhoneNumber = request.PhoneNumber,
                Email = request.Email,
            };

            var queryByUserName = _userManager.Users.Where(p => p.UserName == request.Email);
            var queryByEmail = _userManager.Users.Where(p => p.Email == request.Email);
            var queryByPhoneNumber = _userManager.Users.Where(p => p.PhoneNumber == request.PhoneNumber);

            var user = await queryByUserName
                .Union(queryByEmail)
                .Union(queryByPhoneNumber)
                .FirstOrDefaultAsync(cancellationToken);

            if (user != null)
            {
                model.Display = "display:none;";
                model.Instruct = InstructUserAlreadyExist;
                model.NameContinue = "Đăng Nhập Ngay";
                model.Continue = _appSetting.ResourceContent?.LmsWebsiteUrl;
            }
            else
            {
                var registerUrl = _appSetting.ConstantUrl?.RegisterUrl;
                if (string.IsNullOrEmpty(registerUrl))
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                    return methodResult;
                }
                model.Instruct = InstructUserNotExist;
                model.NameContinue = "Tạo Tài Khoản Ngay";
                model.Continue = string.Format(CultureInfo.InvariantCulture, registerUrl, model.FullName, model.PhoneNumber, model.Email, model.BirthDay);
            }

            await _senderService.SendEmailAsync(new SendEmailByTemplateCommandModel
            {
                ToEmails = new List<string>() { request.Email ?? string.Empty },
                Subject = "Thông báo đăng kí học trải nghiệm",
                Params = model,
                Template = EnumSenderTemplate.MailFromLandingPage,
            });
            return methodResult;
        }

        private class ContactInfo
        {
            public string? FullName { get; set; }
            public string? BirthDay { get; set; }
            public string? PhoneNumber { get; set; }
            public string? Email { get; set; }
            public string? Display { get; set; }
            public string? Instruct { get; set; }
            public string? Continue { get; set; }
            public string? NameContinue { get; set; }
        }
    }
}
