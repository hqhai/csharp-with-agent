// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.LandingPages
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
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

            if (await _eventRegistrationRepository.Queryable.AnyAsync(p => p.Email.ToLower() == request.Email.ToLower(), cancellationToken))
            {
                return methodResult;
            }

            var competitionEvent = await _competitionEventsRepository.Queryable.FirstOrDefaultAsync(p => p.EventCode.ToLower() == request.EventCode.ToLower(), cancellationToken);
            if (competitionEvent == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }
            var eventRegistration = _mapper.Map<EventRegistration>(request);
            eventRegistration.CompetitionEventId = competitionEvent.Id;
            eventRegistration.Status = EnumEventRegistrationStatus.Active;
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
            return methodResult;
        }
    }
}
