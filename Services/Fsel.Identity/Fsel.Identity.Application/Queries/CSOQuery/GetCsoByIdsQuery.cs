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

    public class GetCsoByIdsQuery : IRequest<MethodResult<IList<UserModel>>>
    {
        public IList<Guid>? Ids { get; set; }
    }

    public class GetCsoByIdsQueryHandler : IRequestHandler<GetCsoByIdsQuery, MethodResult<IList<UserModel>>>
    {
        private readonly ICSORepository _csoRepository;

        public GetCsoByIdsQueryHandler(ICSORepository csoRepository)
        {
            _csoRepository = csoRepository;
        }

        public async Task<MethodResult<IList<UserModel>>> Handle(GetCsoByIdsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var methodResult = new MethodResult<IList<UserModel>>();
            if (request.Ids == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Ids));
                return methodResult;
            }
            var users = await _csoRepository.Queryable.Where(p => request.Ids.Contains(p.Id)).Select(x => new UserModel
            {
                Id = x.User!.Id,
                FullName = x.User.FullName,
                Email = x.User.Email,
                PhoneNumber = x.User.PhoneNumber,
                AvatarPath = x.User.AvatarPath,
            }).ToListAsync(cancellationToken);

            methodResult.Result = users;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
