// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.UserConfigs
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.IRepositories;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class CreateUserConfigCommand : IRequest<MethodResult<bool>>
    {
    }

    public class CreateUserConfigCommandHandler : IRequestHandler<CreateUserConfigCommand, MethodResult<bool>>
    {
        private readonly IUserConfigRepository _userConfigRepository;
        private readonly AuthContext _authContext;

        public CreateUserConfigCommandHandler(IUserConfigRepository userConfigRepository, AuthContext authContext)
        {
            _userConfigRepository = userConfigRepository;
            _authContext = authContext;
        }

        public async Task<MethodResult<bool>> Handle(CreateUserConfigCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();
            methodResult.Result = true;
            var userConfig = await _userConfigRepository.Queryable.FirstOrDefaultAsync(p => p.UserId == _authContext.CurrentUserId && p.IsViewNewFeature, cancellationToken);
            if (userConfig == null)
            {
                await _userConfigRepository.ExecuteTransactionAsync(async () =>
                {
                    _userConfigRepository.Add(new UserConfig()
                    {
                        UserId = _authContext.CurrentUserId,
                        IsViewNewFeature = true,
                    });
                    await _userConfigRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                    return methodResult;
                });
            }
            return methodResult;
        }
    }
}
