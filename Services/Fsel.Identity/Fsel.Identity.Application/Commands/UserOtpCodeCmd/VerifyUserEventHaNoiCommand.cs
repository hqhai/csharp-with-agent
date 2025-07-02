// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.UserOtpCodeCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class VerifyUserEventHaNoiCommand : IRequest<MethodResult<bool>>
    {
        public string? PhoneNumber { get; set; }
        public string? EventCode { get; set; }
    }

    public class VerifyUserEventHaNoiCommandHandler : IRequestHandler<VerifyUserEventHaNoiCommand, MethodResult<bool>>
    {
        private readonly IUserOtpCodeRepository _userOtpCodeRepository;
        private readonly UserManager<User> _userManager;
        private readonly ICompetitionEventsRepository _competitionEventsRepository;
        private readonly IStudentCompetitionEventsRepository _studentCompetitionEventsRepository;
        private readonly IMapper _mapper;

        public VerifyUserEventHaNoiCommandHandler(IUserOtpCodeRepository userOtpCodeRepository, UserManager<User> userManager, ICompetitionEventsRepository competitionEventsRepository, IStudentCompetitionEventsRepository studentCompetitionEventsRepository, IMapper mapper)
        {
            _userOtpCodeRepository = userOtpCodeRepository;
            _userManager = userManager;
            _competitionEventsRepository = competitionEventsRepository;
            _studentCompetitionEventsRepository = studentCompetitionEventsRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<bool>> Handle(VerifyUserEventHaNoiCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            if (string.IsNullOrEmpty(request.PhoneNumber) || string.IsNullOrEmpty(request.EventCode))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required), nameof(request.PhoneNumber), request.PhoneNumber);
                return methodResult;
            }

            var user = await _userManager.Users.Include(p => p.Student).FirstOrDefaultAsync(p => p.UserName == request.PhoneNumber, cancellationToken);

            if (user == null)
            {
                var isPhoneNumberAlreadyExist = await _userManager.Users.AnyAsync(p => p.PhoneNumber == request.PhoneNumber, cancellationToken);

                if (isPhoneNumberAlreadyExist)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumOTPCodeErrorCode.PhoneNumberAlreadyExist), nameof(request.PhoneNumber), request.PhoneNumber);
                    return methodResult;
                }
                else
                {
                    methodResult.AddErrorBadRequest(nameof(EnumOTPCodeErrorCode.UserDoesNotExist), nameof(request.PhoneNumber), request.PhoneNumber);
                    return methodResult;
                }
            }

            if (user.Status == EnumUserStatus.Inactive)
            {
                methodResult.AddErrorBadRequest(nameof(EnumOTPCodeErrorCode.PendingVerification), nameof(request.PhoneNumber), request.PhoneNumber);
                return methodResult;
            }

            var student = user.Student;
            var studentId = student?.Id;

            var studentCompetitionEvent = await _studentCompetitionEventsRepository.Queryable.Include(p => p.CompetitionEvents).ThenInclude(p => p.CompetitionEventParent).ThenInclude(p => p.CompetitionEventParent).FirstOrDefaultAsync(p => p.StudentId == studentId, cancellationToken);

            if (studentCompetitionEvent == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumOTPCodeErrorCode.NotInEventHN), nameof(request.PhoneNumber), request.PhoneNumber);
                return methodResult;
            }

            var parentCompetitionEvent = studentCompetitionEvent.CompetitionEvents?.CompetitionEventParent?.CompetitionEventParent;

            if (parentCompetitionEvent == null || parentCompetitionEvent.EventCode != request.EventCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumOTPCodeErrorCode.NotInEventHN), nameof(request.PhoneNumber), request.PhoneNumber);
                return methodResult;
            }

            methodResult.Result = user.EmailConfirmed || user.PhoneNumberConfirmed;
            return methodResult;
        }
    }
}
