// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.LandingPages
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
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

        public FormRegisterStudentForEventCommandHandler(IEventRegistrationRepository eventRegistrationRepository, ICompetitionEventsRepository competitionEventsRepository, IMapper mapper)
        {
            _eventRegistrationRepository = eventRegistrationRepository;
            _competitionEventsRepository = competitionEventsRepository;
            _mapper = mapper;
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
            }
            else
            {
                _mapper.Map(request, eventRegistration);
            }

            if (!eventRegistration.IsValid())
            {
                methodResult.AddError(eventRegistration.ErrorMessages);
                return methodResult;
            }

            await _eventRegistrationRepository.ExecuteTransactionAsync(async () =>
            {
                eventRegistration = _eventRegistrationRepository.Add(eventRegistration);
                await _eventRegistrationRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            });
            methodResult.Result = true;
            return methodResult;
        }
    }
}
