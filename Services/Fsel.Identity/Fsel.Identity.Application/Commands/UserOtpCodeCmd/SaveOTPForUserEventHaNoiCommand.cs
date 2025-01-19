// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.UserOtpCodeCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Application.Services;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public enum EnumActionSaveOTPForEventHaNoi
    {
        Success,
        UpdateInFo,
        LMS
    }

    public class SaveOTPForUserEventHaNoiCommandModel
    {
        public EnumActionSaveOTPForEventHaNoi Action { get; set; }
        public int CountOTP { get; set; }
    }

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

            var user = await _userManager.Users.Include(p => p.UserOtpCodes).Include(p => p.Human).ThenInclude(p => p.Student).FirstOrDefaultAsync(p => p.UserName == request.PhoneNumber, cancellationToken);
            if (user == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumOTPCodeErrorCode.UserDoesNotExist), nameof(request.PhoneNumber), request.PhoneNumber);
                return methodResult;
            }

            if (user.Status == EnumUserStatus.Inactive)
            {
                methodResult.AddErrorBadRequest(nameof(EnumOTPCodeErrorCode.PendingVerification), nameof(request.PhoneNumber), request.PhoneNumber);
                return methodResult;
            }

            var countOTPSMS = user.UserOtpCodes.Where(p => p.Type == EnumUserOtpCodeType.SMS).Count();
            if (countOTPSMS >= 3)
            {
                methodResult.AddErrorBadRequest(nameof(EnumOTPCodeErrorCode.AttemptsExhausted), nameof(request.PhoneNumber), request.PhoneNumber);
                return methodResult;
            }

            var lastOTP = user.UserOtpCodes.Where(p => p.Type == EnumUserOtpCodeType.SMS).OrderByDescending(p => p.CreatedDate).FirstOrDefault();

            if (lastOTP != null && IsValidTime(lastOTP.CreatedDate, DateTime.UtcNow))
            {
                methodResult.AddErrorBadRequest(nameof(EnumOTPCodeErrorCode.SentWithin30Seconds));
                return methodResult;
            }

            var studentId = user.Human?.Student?.Id;

            var studentCompetitionEvent = await _studentCompetitionEventsRepository.Queryable.Include(p => p.CompetitionEvents).FirstOrDefaultAsync(p => p.StudentId == studentId, cancellationToken);
            if (studentCompetitionEvent == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumOTPCodeErrorCode.NotInEventHN), nameof(request.PhoneNumber), request.PhoneNumber);
                return methodResult;
            }

            var competitionEventId = studentCompetitionEvent.CompetitionEvents?.ParentEventId ?? studentCompetitionEvent.CompetitionEvents?.Id;

            var competitionEvent = await _competitionEventsRepository.Queryable.FirstOrDefaultAsync(p => p.Id == competitionEventId, cancellationToken);

            if (competitionEvent == null || competitionEvent.EventCode != request.EventCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumOTPCodeErrorCode.NotInEventHN), nameof(request.PhoneNumber), request.PhoneNumber);
                return methodResult;
            }

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
                var otp = NumberHelper.GetRandomCode();
                var userOtpCode = new UserOtpCode
                {
                    UserId = user.Id,
                    OTPCode = otp,
                    Status = EnumOtpCodeStatus.New,
                    Type = EnumUserOtpCodeType.SMS,
                    ExpiredTime = DateTime.MaxValue,
                };
                _userOtpCodeRepository.Add(userOtpCode);
                await _userOtpCodeRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                var sendSMSResult = await _senderService.SendSMSAsync(new SendSMSCommandModel()
                {
                    PhoneNumbers = new List<string> { request.PhoneNumber },
                    Template = EnumSendSMSTemplate.SendOTP,
                    Params = new
                    {
                        OTP = otp
                    }
                });

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = new SaveOTPForUserEventHaNoiCommandModel { Action = EnumActionSaveOTPForEventHaNoi.LMS, CountOTP = countOTPSMS + 1 };
                return methodResult;
            });
            return methodResult;
        }

        public bool IsValidTime(DateTime dateTime1, DateTime dateTime2)
        {
            TimeSpan difference = dateTime2 - dateTime1;

            return difference.TotalSeconds < 30;
        }
    }
}
