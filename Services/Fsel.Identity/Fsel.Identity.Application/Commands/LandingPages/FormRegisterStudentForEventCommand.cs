// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.LandingPages
{
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Identity.Application.Services;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Infrastructure.ValueSettings;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class FormRegisterStudentForEventCommand : RegisterStudentForEventCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class FormRegisterStudentForEventCommandHandler : IRequestHandler<FormRegisterStudentForEventCommand, MethodResult<bool>>
    {
        private readonly IEventRegistrationRepository _eventRegistrationRepository;
        private readonly ICompetitionEventsRepository _competitionEventsRepository;
        private readonly IMapper _mapper;
        private readonly ISenderService _senderService;
        private readonly AppSetting _appSetting;
        private const string Subject = "Thông tin đăng kí tham gia sự kiện";

        public FormRegisterStudentForEventCommandHandler(IEventRegistrationRepository eventRegistrationRepository, ICompetitionEventsRepository competitionEventsRepository, IMapper mapper, ISenderService senderService, AppSetting appSetting)
        {
            _eventRegistrationRepository = eventRegistrationRepository;
            _competitionEventsRepository = competitionEventsRepository;
            _mapper = mapper;
            _senderService = senderService;
            _appSetting = appSetting;
        }

        public async Task<MethodResult<bool>> Handle(FormRegisterStudentForEventCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var competitionEvent = await _competitionEventsRepository.Queryable.FirstOrDefaultAsync(p => p.EventCode.ToLower() == request.EventCode.ToLower(), cancellationToken);
            if (competitionEvent == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            if (!string.IsNullOrEmpty(request.Email) && !request.Email.IsValidEmail())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat));
                return methodResult;
            }
            if (!string.IsNullOrEmpty(request.PhoneNumber) && !request.PhoneNumber.IsValidPhoneNumber())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat));
                return methodResult;
            }

            var eventRegistration = await _eventRegistrationRepository.Queryable.FirstOrDefaultAsync(p => p.Email.ToLower() == request.Email.ToLower() && p.CompetitionEventId == competitionEvent.Id, cancellationToken);

            if (eventRegistration == null)
            {
                eventRegistration = _mapper.Map<EventRegistration>(request);
                eventRegistration.CompetitionEventId = competitionEvent.Id;
                eventRegistration.Status = EnumEventRegistrationStatus.Active;
                eventRegistration = _eventRegistrationRepository.Add(eventRegistration);
            }
            else
            {
                _mapper.Map(request, eventRegistration);
                eventRegistration = _eventRegistrationRepository.Update(eventRegistration);
            }

            if (!eventRegistration.IsValid())
            {
                methodResult.AddError(eventRegistration.ErrorMessages);
                return methodResult;
            }

            await _eventRegistrationRepository.ExecuteTransactionAsync(async () =>
            {
                await _eventRegistrationRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                await SendMail(request, competitionEvent, EnumSenderTemplate.MailRegisterEvent, Subject, CultureInfo.InvariantCulture).ConfigureAwait(false);
                return methodResult;
            });

            methodResult.Result = true;
            return methodResult;
        }

        private async Task SendMail(RegisterStudentForEventCommandModel request, CompetitionEvent competitionEvent, EnumSenderTemplate senderTemplate, string subject, CultureInfo cultureInfo)
        {
            var param = new ParamSendMailEvent
            {
                FullName = request.FirstName + " " + request.LastName,
                LinkLMS = _appSetting.ResourceContent?.LmsWebsiteUrl,
                StartDateEvent = competitionEvent.EventContent?.StartDate?.ToString("dd/MM", cultureInfo),
                EndDateEvent = competitionEvent.EventContent?.EndDate?.ToString("dd/MM/yyyy", cultureInfo),
                StartDateAward = competitionEvent.EventContent?.AwardStartDate?.ToString("dd/MM", cultureInfo),
                EndDateAward = competitionEvent.EventContent?.AwardEndDate?.ToString("dd/MM/yyyy", cultureInfo),
                Date = competitionEvent.EventContent?.StartDate?.ToString("dd/MM/yyyy", cultureInfo),
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                BirthDay = request.BirthDay.ToString("dd/MM/yyyy", cultureInfo),
                School = request.School,
                SchoolStudentCode = request.SchoolStudentCode,
                SchoolGrade = request.SchoolGrade,
                SchoolClass = request.SchoolClass,
                LinkLeaderBoard = competitionEvent.EventContent?.LinkLeaderBoard
            };

            await _senderService.SendEmailAsync(new SendEmailByTemplateCommandModel
            {
                ToEmails = new List<string>() { request.Email ?? string.Empty },
                Template = senderTemplate,
                Subject = subject,
                Params = param,
            });
        }
    }
}
