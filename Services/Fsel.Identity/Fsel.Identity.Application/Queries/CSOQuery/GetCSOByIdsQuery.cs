// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.CSOQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetCSOByIdsQuery : IRequest<MethodResult<IList<HumanModel>>>
    {
        public IList<Guid> Ids { get; set; }
    }

    public class GetCSOByIdsQueryHandler : IRequestHandler<GetCSOByIdsQuery, MethodResult<IList<HumanModel>>>
    {
        private readonly ICSORepository _csoRepository;

        public GetCSOByIdsQueryHandler(ICSORepository csoRepository)
        {
            _csoRepository = csoRepository;
        }

        public async Task<MethodResult<IList<HumanModel>>> Handle(GetCSOByIdsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            MethodResult<IList<HumanModel>> methodResult = new MethodResult<IList<HumanModel>>();
            if (request.Ids == null)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var csos = await _csoRepository.Queryable.Where(p => request.Ids.Contains(p.Id)).Include(i => i.Human).Select(x => new HumanModel
            {
                Id = x.Id,
                FullName = x.Human!.FullName
            }).ToListAsync(cancellationToken);

            methodResult.Result = csos;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
