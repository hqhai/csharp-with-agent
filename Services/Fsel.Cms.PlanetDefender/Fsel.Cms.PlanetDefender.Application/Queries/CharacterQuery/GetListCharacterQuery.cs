// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Queries.CharacterQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Cms.PlanetDefender.Domain.IRepositories;
    using Fsel.Cms.PlanetDefender.Domain.Models.EntityModels;
    using Fsel.Cms.PlanetDefender.Infrastructure.Repositories;
    using Fsel.Common.ActionResults;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetListCharacterQuery : IRequest<MethodResult<IList<SpaceShipModel>>>
    {
        public bool? IsDefault { get; set; }
    }
    public class GetListCharacterQueryHandler : IRequestHandler<GetListCharacterQuery, MethodResult<IList<SpaceShipModel>>>
    {
        private readonly ICharacterRepository _characterRepository;
        private readonly IMapper _mapper;
        public GetListCharacterQueryHandler(ICharacterRepository characterRepository, IMapper mapper)
        {
            _characterRepository = characterRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<SpaceShipModel>>> Handle(GetListCharacterQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<SpaceShipModel>>();

            var character = await _characterRepository.Queryable.Select(p => _mapper.Map<SpaceShipModel>(p)).ToListAsync(cancellationToken);

            if (request.IsDefault.HasValue)
            {
                character = character.Where(x => x.IsDefault == request.IsDefault).ToList();
            }

            methodResult.Result = character;
            return methodResult;

        }
    }
}
