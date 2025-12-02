// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.UserOtpCodeCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Application.Services.SenderService;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.UserOtpCodes;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SaveOTPForUserEventHaNoiCommand : IRequest<MethodResult<SaveOTPForUserEventHaNoiCommandModel>>
    {
        public string? PhoneNumber { get; set; }
        public string? EventCode { get; set; }
    }

    public class SaveOTPForUserEventHaNoiCommandHandler : IRequestHandler<SaveOTPForUserEventHaNoiCommand, MethodResult<SaveOTPForUserEventHaNoiCommandModel>>
    {
        private readonly IUserOtpCodeRepository _userOtpCodeRepository;
        private readonly UserManager<User> _userManager;
        private readonly ICompetitionEventsRepository _competitionEventsRepository;
        private readonly IStudentCompetitionEventsRepository _studentCompetitionEventsRepository;
        private readonly ISenderService _senderService;

        public SaveOTPForUserEventHaNoiCommandHandler(IUserOtpCodeRepository userOtpCodeRepository, UserManager<User> userManager, ICompetitionEventsRepository competitionEventsRepository, IStudentCompetitionEventsRepository studentCompetitionEventsRepository, ISenderService senderService)
        {
            _userOtpCodeRepository = userOtpCodeRepository;
            _userManager = userManager;
            _competitionEventsRepository = competitionEventsRepository;
            _studentCompetitionEventsRepository = studentCompetitionEventsRepository;
            _senderService = senderService;
        }

        public async Task<MethodResult<SaveOTPForUserEventHaNoiCommandModel>> Handle(SaveOTPForUserEventHaNoiCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<SaveOTPForUserEventHaNoiCommandModel>();

            if (string.IsNullOrEmpty(request.PhoneNumber) || string.IsNullOrEmpty(request.EventCode))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required), nameof(request.PhoneNumber), request.PhoneNumber);
                return methodResult;
            }

            var user = await _userManager.Users.FirstOrDefaultAsync(p => p.UserName == request.PhoneNumber, cancellationToken);
            if (user == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumOTPCodeErrorCode.UserDoesNotExist), nameof(request.PhoneNumber), request.PhoneNumber);
                return methodResult;
            }

            var lastOTP = await _userOtpCodeRepository.Queryable.Where(p => p.UserId == user.Id && p.Type == EnumUserOtpCodeType.SMS).OrderByDescending(p => p.CreatedDate).FirstOrDefaultAsync(cancellationToken);

            if (lastOTP != null && lastOTP.Status == EnumOtpCodeStatus.New && lastOTP.RetryCount >= 3)
            {
                methodResult.AddErrorBadRequest(nameof(EnumOTPCodeErrorCode.AttemptsExhausted), nameof(request.PhoneNumber), request.PhoneNumber);
                return methodResult;
            }

            //if (lastOTP != null && IsValidTime(lastOTP.UpdatedDate ?? lastOTP.CreatedDate, DateTime.UtcNow))
            //{
            //    methodResult.AddErrorBadRequest(nameof(EnumOTPCodeErrorCode.SentWithin30Seconds));
            //    return methodResult;
            //}

            if (lastOTP != null && lastOTP.Type == EnumUserOtpCodeType.SMS && lastOTP.Status == EnumOtpCodeStatus.Verified && !user.PhoneNumberConfirmed && !user.EmailConfirmed)
            {
                methodResult.Result = new SaveOTPForUserEventHaNoiCommandModel { Action = EnumActionSaveOTPForEventHaNoi.UpdateInFo, CountOTP = 0 };
                return methodResult;
            }
            else if (lastOTP != null && lastOTP.Type == EnumUserOtpCodeType.SMS && lastOTP.Status == EnumOtpCodeStatus.Verified && user.PhoneNumberConfirmed && user.EmailConfirmed)
            {
                methodResult.Result = new SaveOTPForUserEventHaNoiCommandModel { Action = EnumActionSaveOTPForEventHaNoi.LMS, CountOTP = 0 };
                return methodResult;
            }

            await _userOtpCodeRepository.ExecuteTransactionAsync(async () =>
            {
                if (lastOTP == null)
                {
                    var otp = NumberHelper.GetRandomCode();
                    lastOTP = new UserOtpCode
                    {
                        UserId = user.Id,
                        OtpCode = otp,
                        Status = EnumOtpCodeStatus.New,
                        Type = EnumUserOtpCodeType.SMS,
                        RetryCount = 1,
                        ExpiredTime = DateTime.MaxValue,
                    };
                    _userOtpCodeRepository.Add(lastOTP);
                }
                else
                {
                    lastOTP.RetryCount += 1;
                    lastOTP = _userOtpCodeRepository.Update(lastOTP);
                }

                await _userOtpCodeRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                var sendSMSResult = await _senderService.SendSMSAsync(new SendSMSCommandModel()
                {
                    PhoneNumbers = new List<string> { request.PhoneNumber },
                    Template = EnumSendSMSTemplate.SendOTP,
                    Params = new
                    {
                        CountOTP = lastOTP.RetryCount,
                        OTP = lastOTP.OtpCode,
                    },
                    IsCheckDuplicate = false,
                });

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = new SaveOTPForUserEventHaNoiCommandModel { Action = EnumActionSaveOTPForEventHaNoi.Success, CountOTP = lastOTP.RetryCount };
                return methodResult;
            });
            return methodResult;
        }

        //public bool IsValidTime(DateTime dateTime1, DateTime dateTime2)
        //{
        //    TimeSpan difference = dateTime2 - dateTime1;

        //    return difference.TotalSeconds < 30;
        //}
    }
}
