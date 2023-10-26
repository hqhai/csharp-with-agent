// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Queries.SpaceShipQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Cms.PlanetDefender.Domain.IRepositories;
    using Fsel.Cms.PlanetDefender.Domain.Models.EntityModels;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchSpaceShipQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<SpaceShipModel>>>
    {
        public bool IsDefault { get; set; }
    }

    public class SearchSpaceShipQueryHandler : IRequestHandler<SearchSpaceShipQuery, MethodResult<PagingItemsModel<SpaceShipModel>>>
    {
        private readonly ISpaceShipRepository _spaceShipRepository;

        public SearchSpaceShipQueryHandler(ISpaceShipRepository spaceShipRepository)
        {
            _spaceShipRepository = spaceShipRepository;
        }

        public async Task<MethodResult<PagingItemsModel<SpaceShipModel>>> Handle(SearchSpaceShipQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<SpaceShipModel>>();

            var spaceShip = _spaceShipRepository
                        .Queryable
                        .Where(x => x.IsDefault == request.IsDefault)
                        .Select(x => new SpaceShipModel
                        {
                            Id = x.Id,
                            Name = x.Name,
                            Code = x.Code,
                            IsDefault = x.IsDefault,
                            CreatedDate = x.CreatedDate,
                            CreatedFullName = x.CreatedFullName,
                        });

            int totalItem = await spaceShip.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await spaceShip
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            methodResult.Result = new PagingItemsModel<SpaceShipModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
