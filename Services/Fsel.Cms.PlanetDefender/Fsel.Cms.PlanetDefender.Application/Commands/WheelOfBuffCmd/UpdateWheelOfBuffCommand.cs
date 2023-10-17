// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Commands.WheelOfBuffCmd
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Cms.PlanetDefender.Domain.Entities;
    using Fsel.Cms.PlanetDefender.Domain.IRepositories;
    using Fsel.Cms.PlanetDefender.Domain.Models.CommandModel;
    using Fsel.Common.ActionResults;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateWheelOfBuffCommand : UpdateWheelOfBuffCommandModel, IRequest<MethodResult<IList<WheelOfBuff>>>
    {

    }
    public class UpdateWheelOfBuffCommandHandler : IRequestHandler<UpdateWheelOfBuffCommand, MethodResult<IList<WheelOfBuff>>>
    {
        private readonly IWheelOfBuffRepository _wheelOfBuffRepository;
        private readonly IMapper _mapper;

        public UpdateWheelOfBuffCommandHandler(IWheelOfBuffRepository wheelOfBuffRepository, IMapper mapper)
        {
            _wheelOfBuffRepository = wheelOfBuffRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<WheelOfBuff>>> Handle(UpdateWheelOfBuffCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<WheelOfBuff>> methodResult = new MethodResult<IList<WheelOfBuff>>();
            IList<WheelOfBuff> wheelOfBuff = (await _wheelOfBuffRepository.Queryable.Select(p => new WheelOfBuff
            {
                Id = p.Id,
                IsActive = p.IsActive,
                Type = p.Type
            }).ToListAsync(cancellationToken)).ToList();

            _mapper.Map(request.WheelOfBuffCommandModels, wheelOfBuff);
            await _wheelOfBuffRepository.ExecuteTransactionAsync(async () =>
            {
                _wheelOfBuffRepository.UpdateList(wheelOfBuff);
                await _wheelOfBuffRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = wheelOfBuff;
                return methodResult;
            });


            return methodResult;
        }
    }
}
