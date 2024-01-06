// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Queries.SpaceShipQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Cms.PlanetDefender.Domain.IRepositories;
    using Fsel.Cms.PlanetDefender.Domain.Models.EntityModels;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using MediatR;

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
            var spaceShipsQuery = _spaceShipRepository.Queryable.Where(x => x.IsDefault == request.IsDefault);
            return await _spaceShipRepository.GetListByPageResultAsync<SpaceShipModel>(spaceShipsQuery, request, cancellationToken).ConfigureAwait(false);
        }
    }
}
