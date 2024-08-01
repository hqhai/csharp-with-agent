// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.CompetitionEventsCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class CreateCompetitionEventsCommand : IRequest<MethodResult<CompetitionEventsModel>>
    {
        public string? EventCode { get; set; }

        public string? EventContentStr { get; set; }
    }

    public class CreateCompetitionEventsCommandHandler : IRequestHandler<CreateCompetitionEventsCommand, MethodResult<CompetitionEventsModel>>
    {
        private readonly ICompetitionEventsRepository _competitionEventsRepository;
        private readonly IMapper _mapper;
        public CreateCompetitionEventsCommandHandler(ICompetitionEventsRepository competitionEventsRepository, IMapper mapper)
        {
            _competitionEventsRepository = competitionEventsRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<CompetitionEventsModel>> Handle(CreateCompetitionEventsCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<CompetitionEventsModel> methodResult = new MethodResult<CompetitionEventsModel>();
            var existsCompetitionEvent = _competitionEventsRepository.Queryable.Where(x => x.EventCode == request.EventCode);
            if (existsCompetitionEvent.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist));
                return methodResult;
            }

            CompetitionEvents competitionEvents = new CompetitionEvents
            {
                EventCode = request.EventCode,
                EventContentStr = request.EventContentStr,
            };

            await _competitionEventsRepository.ExecuteTransactionAsync(async () =>
            {
                _competitionEventsRepository.Add(competitionEvents);
                await _competitionEventsRepository.UnitOfWork.SaveEntitiesAsync().ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<CompetitionEventsModel>(competitionEvents);
                return methodResult;
            });


            return methodResult;
        }


    }
}
