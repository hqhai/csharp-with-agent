// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.BlindBoxes
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.System.Domain.Entities.BlindBoxs;
    using Fsel.System.Domain.IRepositories.BlindBoxes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateBlindBoxesCommand : IRequest<MethodResult<int>>
    {
        public IList<Guid>? UserIds { get; set; }
    }

    public class CreateBlindBoxesCommandHandler : IRequestHandler<CreateBlindBoxesCommand, MethodResult<int>>
    {
        private readonly IBlindBoxRepository _blindBoxRepository;
        private readonly IBlindBoxUserRepository _blindBoxUserRepository;

        public CreateBlindBoxesCommandHandler(IBlindBoxRepository blindBoxRepository,
            IBlindBoxUserRepository blindBoxUserRepository)
        {
            _blindBoxRepository = blindBoxRepository;
            _blindBoxUserRepository = blindBoxUserRepository;
        }

        public async Task<MethodResult<int>> Handle(CreateBlindBoxesCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<int> methodResult = new MethodResult<int>();
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
                                                                      .Where(x => x.BlindBoxId == blindBox.Id).ToListAsync(cancellationToken);
            var userIds = request.UserIds.Where(x => !blindBoxUsers.Select(x => x.UserId).Contains(x)).ToList();
            var listBlindBoxUser = userIds.Select(x => new BlindBoxUser
            {
                UserId = x,
                BlindBoxId = blindBox.Id,
            }).ToList();
            if (listBlindBoxUser.Any())
            {
                await _blindBoxUserRepository.AddList(listBlindBoxUser);
                await _blindBoxUserRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            methodResult.Result = await _blindBoxUserRepository.Queryable.WhereBulkContains(request.UserIds, x => x.UserId).Where(x => x.BlindBoxId == blindBox.Id).CountAsync(cancellationToken);
            return methodResult;
        }
    }
}
