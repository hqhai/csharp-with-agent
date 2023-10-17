// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Queries.WheelOfBuff
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Cms.PlanetDefender.Domain.Helpers;
    using Fsel.Cms.PlanetDefender.Domain.IRepositories;
    using Fsel.Cms.PlanetDefender.Domain.Models.EntityModels;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetWheelOfBuffQuery : IRequest<MethodResult<IList<WheelOfBuffModel>>>
    {
    }
    public class GetWheelOfBuffQueryHandler : IRequestHandler<GetWheelOfBuffQuery, MethodResult<IList<WheelOfBuffModel>>>
    {
        private readonly IWheelOfBuffRepository _wheelOfBuffRepository;

        public GetWheelOfBuffQueryHandler(IWheelOfBuffRepository wheelOfBuffRepository)
        {
            _wheelOfBuffRepository = wheelOfBuffRepository;
        }

        public async Task<MethodResult<IList<WheelOfBuffModel>>> Handle(GetWheelOfBuffQuery request, CancellationToken cancellationToken)
        {
            MethodResult<IList<WheelOfBuffModel>> methodResult = new MethodResult<IList<WheelOfBuffModel>>();
            ArgumentNullException.ThrowIfNull(request);
            var wheelOfBuffs = await _wheelOfBuffRepository.Queryable.Select(x=>new WheelOfBuffModel
            {
                Id = x.Id,
                IsActive=x.IsActive,
                Name = EnumHelper.GetDescription(x.Type)
            }).ToListAsync(cancellationToken);
            methodResult.Result = wheelOfBuffs;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;

        }
    }
}
