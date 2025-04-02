// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.BlindBoxes
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.System.Domain.IRepositories.BlindBoxes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetBlindBoxesByUserIdsQuery : IRequest<MethodResult<IList<Guid>>>
    {
        public IList<Guid>? UserIds { get; set; }
    }

    public class GetBlindBoxesByUserIdsQueryHandler : IRequestHandler<GetBlindBoxesByUserIdsQuery, MethodResult<IList<Guid>>>
    {
        private readonly IBlindBoxRepository _blindBoxRepository;
        private readonly IBlindBoxUserRepository _blindBoxUserRepository;

        public GetBlindBoxesByUserIdsQueryHandler(IBlindBoxRepository blindBoxRepository, IBlindBoxUserRepository blindBoxUserRepository)
        {
            _blindBoxRepository = blindBoxRepository;
            _blindBoxUserRepository = blindBoxUserRepository;
        }

        public async Task<MethodResult<IList<Guid>>> Handle(GetBlindBoxesByUserIdsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<Guid>>();
            if (request.UserIds == null || request.UserIds.Count == 0)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
            var blindBox = await _blindBoxRepository.Queryable.FirstOrDefaultAsync(cancellationToken);
            if (blindBox == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(blindBox));
                return methodResult;
            }
            var blindBoxUsers = await _blindBoxUserRepository.Queryable.WhereBulkContains(request.UserIds, x => x.UserId)
                                                                       .Where(x => x.BlindBoxId == blindBox.Id)
                                                                       .ToListAsync(cancellationToken);
            methodResult.Result = blindBoxUsers.Select(x => x.UserId).ToList();
            return methodResult;
        }
    }
}
