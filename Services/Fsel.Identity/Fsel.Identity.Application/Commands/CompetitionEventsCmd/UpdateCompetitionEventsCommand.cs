// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.CompetitionEventsCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.CompetitionEvent;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class UpdateCompetitionEventsCommand : CreateCompetitionEventCommandModel, IRequest<MethodResult<CompetitionEventsModel>>
    {

    }

    public class UpdateCompetitionEventsCommandHandler : IRequestHandler<UpdateCompetitionEventsCommand, MethodResult<CompetitionEventsModel>>
    {
        private readonly ICompetitionEventsRepository _competitionEventsRepository;
        private readonly IMapper _mapper;
        public UpdateCompetitionEventsCommandHandler(ICompetitionEventsRepository competitionEventsRepository, IMapper mapper)
        {
            _competitionEventsRepository = competitionEventsRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<CompetitionEventsModel>> Handle(UpdateCompetitionEventsCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<CompetitionEventsModel> methodResult = new MethodResult<CompetitionEventsModel>();
            var existsCompetitionEvent = _competitionEventsRepository.Queryable.FirstOrDefault(x => x.EventCode == request.EventCode);

            if (existsCompetitionEvent == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var competitionEvents = _mapper.Map(request, existsCompetitionEvent);

            await _competitionEventsRepository.ExecuteTransactionAsync(async () =>
            {
                _competitionEventsRepository.Update(competitionEvents);
                await _competitionEventsRepository.UnitOfWork.SaveEntitiesAsync().ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<CompetitionEventsModel>(competitionEvents);
                return methodResult;
            });


            return methodResult;
        }


    }
}
