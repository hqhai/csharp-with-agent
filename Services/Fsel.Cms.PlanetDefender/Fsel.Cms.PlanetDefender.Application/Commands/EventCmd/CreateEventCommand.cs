// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Commands.EventCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Cms.PlanetDefender.Domain.Entities;
    using Fsel.Cms.PlanetDefender.Domain.IRepositories;
    using Fsel.Cms.PlanetDefender.Domain.Models.CommandModel.NewsAndUpdates;
    using Fsel.Cms.PlanetDefender.Domain.Models.EntityModels;
    using Fsel.Common.ActionResults;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class CreateEventCommand : CreateEventCommandModel, IRequest<MethodResult<EventModel>>
    {
    }

    public class CreateEventCommandHandler : IRequestHandler<CreateEventCommand, MethodResult<EventModel>>
    {
        private readonly IMapper _mapper;
        private readonly IEventRepository _newsAndUpdateRepository;

        public CreateEventCommandHandler(IMapper mapper, IEventRepository newsAndUpdateRepository)
        {
            _mapper = mapper;
            _newsAndUpdateRepository = newsAndUpdateRepository;
        }

        public async Task<MethodResult<EventModel>> Handle(CreateEventCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<EventModel>();

            var newsAndUpdate = _mapper.Map<Event>(request);

            await _newsAndUpdateRepository.ExecuteTransactionAsync(async () =>
            {
                newsAndUpdate = _newsAndUpdateRepository.Add(newsAndUpdate);
                await _newsAndUpdateRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<EventModel>(newsAndUpdate);
                return methodResult;
            });

            return methodResult;
        }
    }
}
