// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.UserOtpCodeCmd
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Application.Services.SenderService;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.UserOtpCodes;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SendOtpForPhoneCheckUserCommand : IRequest<MethodResult<SaveOTPForUserEventHaNoiCommandModel>>
    {
        public string? PhoneNumber { get; set; }
        public bool IsSMS { get; set; }
    }

    public class SendOtpForPhoneCheckUserCommandHandler : IRequestHandler<SendOtpForPhoneCheckUserCommand, MethodResult<SaveOTPForUserEventHaNoiCommandModel>>
    {
        private readonly IUserOtpCodeRepository _userOtpCodeRepository;
        private readonly UserManager<User> _userManager;
        private readonly ISenderService _senderService;
        private readonly IStudentRepository _studentRepository;
        private readonly IStudentCompetitionEventsRepository _studentCompetitionEventsRepository;
        private readonly ICompetitionEventsRepository _competitionEventsRepository;

        public SendOtpForPhoneCheckUserCommandHandler(IUserOtpCodeRepository userOtpCodeRepository,
            UserManager<User> userManager,
            ISenderService senderService,
            IStudentRepository studentRepository,
            IStudentCompetitionEventsRepository studentCompetitionEventsRepository,
            ICompetitionEventsRepository competitionEventsRepository)
        {
            _userOtpCodeRepository = userOtpCodeRepository;
            _userManager = userManager;
            _senderService = senderService;
            _studentRepository = studentRepository;
            _studentCompetitionEventsRepository = studentCompetitionEventsRepository;
            _competitionEventsRepository = competitionEventsRepository;
        }

        public async Task<MethodResult<SaveOTPForUserEventHaNoiCommandModel>> Handle(SendOtpForPhoneCheckUserCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<SaveOTPForUserEventHaNoiCommandModel>();

            if (string.IsNullOrEmpty(request.PhoneNumber))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.PhoneNumber));
                return methodResult;
            }

            if (!request.PhoneNumber.IsValidPhoneNumber())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(request.PhoneNumber), request.PhoneNumber);
                return methodResult;
            }
            var users = await _userManager.Users.Where(x => x.PhoneNumber == request.PhoneNumber).ToListAsync(cancellationToken);
            var user = users.FirstOrDefault();

            if (users.Count > 1 || user == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.PhoneNumber), request.PhoneNumber);
                return methodResult;
            }

            var query = await (from u in _userManager.Users
                               join s in _studentRepository.Queryable on u.Id equals s.UserId
                               where u.Id == user.Id
                               select new
                               {
                                   User = u,
                                   Student = s
                               }).FirstOrDefaultAsync(cancellationToken);

            if (query == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(query.User));
                return methodResult;
            }

            var userOtpCode = await _userOtpCodeRepository.Queryable.Where(p => p.UserId == query.User.Id && p.Type == EnumUserOtpCodeType.SMS)
                                                                .Where(x => x.Status == EnumOtpCodeStatus.New)
                                                                .FirstOrDefaultAsync(cancellationToken);

            if (userOtpCode != null && userOtpCode.RetryCount >= ValueSettings.Retrycount)
            {
                methodResult.AddErrorBadRequest(nameof(EnumOTPCodeErrorCode.AttemptsExhausted), nameof(query.User.PhoneNumber), query.User.PhoneNumber);
                return methodResult;
            }

            var competitionEvents = await (from ce in _competitionEventsRepository.Queryable
                                           join sce in _studentCompetitionEventsRepository.Queryable on ce.Id equals sce.CompetitionEventId
                                           where sce.StudentId == query.Student.Id
                                           select ce).ToListAsync(cancellationToken);

            var competitionEvent = competitionEvents.OrderByDescending(p => p.CreatedDate).FirstOrDefault();

            await _userOtpCodeRepository.ExecuteTransactionAsync(async () =>
            {
                if (userOtpCode == null)
                {
                    var defaultOtp = competitionEvent?.EventContent?.DefaultOtp;

                    var otp = string.Empty;

                    if (!string.IsNullOrEmpty(defaultOtp))
                    {
                        otp = defaultOtp;
                    }
                    else
                    {
                        otp = NumberHelper.GetRandomCode();
                    }

                    userOtpCode = new UserOtpCode
                    {
                        UserId = query.User.Id,
                        OtpCode = otp,
                        Status = EnumOtpCodeStatus.New,
                        Type = EnumUserOtpCodeType.SMS,
                        RetryCount = 1,
                        ExpiredTime = DateTime.UtcNow.AddMinutes(5),
                    };
                    _userOtpCodeRepository.Add(userOtpCode);
                }
                else
                {
                    userOtpCode.RetryCount += 1;
                    userOtpCode = _userOtpCodeRepository.Update(userOtpCode);
                }

                await _userOtpCodeRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                if (request.IsSMS)
                {
                    var sendSMSResult = await _senderService.SendSMSAsync(new SendSMSCommandModel()
                    {
                        PhoneNumbers = new List<string> { query.User.PhoneNumber ?? string.Empty },
                        Template = EnumSendSMSTemplate.SendOTP,
                        Params = new
                        {
                            OTP = userOtpCode.OtpCode,
                            CountOTP = userOtpCode.RetryCount
                        },
                        IsCheckDuplicate = false,
                    });
                }
                else
                {
                    var sendSMSResult = await _senderService.SendSMSWithZaloAsync(new SendSMSByZaloCommandModel()
                    {
                        PhoneNumbers = new List<string> { query.User.PhoneNumber ?? string.Empty },
                        Type = 1,
                        Template = EnumZaloTemplate.OTP,
                        Params = new
                        {
                            otp = userOtpCode.OtpCode
                        },
                        UseUnicode = 0
                    });
                }

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = new SaveOTPForUserEventHaNoiCommandModel { Action = EnumActionSaveOTPForEventHaNoi.Success, CountOTP = userOtpCode.RetryCount };
                return methodResult;
            });
            return methodResult;
        }
    }
}
