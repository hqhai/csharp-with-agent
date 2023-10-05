// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.UserCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.Users;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class ChangeAccountStatusCommand : ChangeAccountStatusCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class ChangeAccountStatusCommandHandler : IRequestHandler<ChangeAccountStatusCommand, MethodResult<bool>>
    {
        private readonly IPlatformRepository _platformRepository;
        private readonly IUserPlatformRepository _userPlatformRepository;

        public ChangeAccountStatusCommandHandler(IPlatformRepository platformRepository, IUserPlatformRepository userPlatformRepository)
        {
            _platformRepository = platformRepository;
            _userPlatformRepository = userPlatformRepository;
        }

        public async Task<MethodResult<bool>> Handle(ChangeAccountStatusCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();
            if (request.UserId == null || request.UserId.Count == 0)
            {
                return methodResult;
            }
            var platform = await _platformRepository.Queryable.FirstOrDefaultAsync(p => p.Code == request.PlatformCode, cancellationToken);
            if (platform == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumPlatFormErrorCode.PlatFormNotExist));
                return methodResult;
            }

            var userPlatforms = await _userPlatformRepository.Queryable.Where(p => request.UserId.Contains(p.UserId ?? string.Empty) && p.PlatformId == platform.Id).ToListAsync(cancellationToken);

            userPlatforms.ForEach(p => { p.Status = request.Status; });

            await _userPlatformRepository.ExecuteTransactionAsync(async () =>
            {
                _userPlatformRepository.UpdateList(userPlatforms);
                await _userPlatformRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });
            return methodResult;
        }
    }
}
