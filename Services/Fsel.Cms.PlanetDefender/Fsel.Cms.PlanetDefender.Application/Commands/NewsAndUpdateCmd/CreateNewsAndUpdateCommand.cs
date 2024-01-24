// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Commands.NewsAndUpdateCmd
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Cms.PlanetDefender.Domain.Entities;
    using Fsel.Cms.PlanetDefender.Domain.IRepositories;
    using Fsel.Cms.PlanetDefender.Domain.Models.CommandModel.NewsAndUpdates;
    using Fsel.Cms.PlanetDefender.Domain.Models.EntityModels;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class CreateNewsAndUpdateCommand : CreateNewsAndUpdateCommandModel, IRequest<MethodResult<NewsAndUpdateModel>>
    {
    }
    public class CreateNewsAndUpdateCommandHandler : IRequestHandler<CreateNewsAndUpdateCommand, MethodResult<NewsAndUpdateModel>>
    {
        private readonly IMapper _mapper;
        private readonly INewsAndUpdateRepository _newsAndUpdateRepository;

        public CreateNewsAndUpdateCommandHandler(IMapper mapper, INewsAndUpdateRepository newsAndUpdateRepository)
        {
            _mapper = mapper;
            _newsAndUpdateRepository = newsAndUpdateRepository;
        }

        public async Task<MethodResult<NewsAndUpdateModel>> Handle(CreateNewsAndUpdateCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<NewsAndUpdateModel> methodResult = new MethodResult<NewsAndUpdateModel>();

            NewsAndUpdate newsAndUpdate = _mapper.Map<NewsAndUpdate>(request);

            await _newsAndUpdateRepository.ExecuteTransactionAsync(async () =>
            {
                newsAndUpdate = _newsAndUpdateRepository.Add(newsAndUpdate);
                await _newsAndUpdateRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<NewsAndUpdateModel>(newsAndUpdate);
                return methodResult;
            });

            return methodResult;
        }
    }
}
