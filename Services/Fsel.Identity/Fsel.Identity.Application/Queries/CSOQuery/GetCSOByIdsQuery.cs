// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.CSOQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetCSOByIdsQuery : IRequest<MethodResult<IList<HumanModel>>>
    {
        public IList<Guid>? Ids { get; set; }
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

            var methodResult = new MethodResult<IList<HumanModel>>();
            if (request.Ids == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Ids));
                return methodResult;
            }
            IList<HumanModel> human = new List<HumanModel>();
            human = await _csoRepository.Queryable.Where(p => request.Ids.Contains(p.Id)).Include(i => i.Human).Select(x => new HumanModel
            {
                Id = x.Id,
                FullName = x.Human!.FullName,
                Email = x.Human!.Email,
                PhoneNumber = x.Human!.PhoneNumber,

            }).ToListAsync(cancellationToken);

            methodResult.Result = human;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
