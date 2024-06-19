// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.UserQuery
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Identity.Domain.Models.QueryModels.Users;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetUsersByIdsQuery : GetUsersByIdsQueryModel, IRequest<MethodResult<IList<HumanModel>>>
    {
    }

    public class GetUsersByIdsQueryHandler : IRequestHandler<GetUsersByIdsQuery, MethodResult<IList<HumanModel>>>
    {
        private readonly IMapper _mapper;
        private readonly IHumanRepository _humanRepository;

        public GetUsersByIdsQueryHandler(IMapper mapper, IHumanRepository humanRepository)
        {
            _mapper = mapper;
            _humanRepository = humanRepository;
        }

        public async Task<MethodResult<IList<HumanModel>>> Handle(GetUsersByIdsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<HumanModel>> methodResult = new MethodResult<IList<HumanModel>>();

            if (request.UserIds == null || !request.UserIds.Any())
            {
                methodResult.Result = new List<HumanModel>();
                return methodResult;
            }
            var humans = await _humanRepository.Queryable.Where(p => p.UserId.HasValue && request.UserIds.Contains(p.UserId.Value)).ToListAsync(cancellationToken);
            methodResult.Result = _mapper.Map<IList<HumanModel>>(humans);
            return methodResult;
        }
    }
}
