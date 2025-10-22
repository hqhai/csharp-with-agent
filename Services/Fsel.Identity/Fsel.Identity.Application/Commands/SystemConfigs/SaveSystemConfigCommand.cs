// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.SystemConfigs
{
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SaveSystemConfigCommand : IRequest<MethodResult<bool>>
    {
        public bool IsEnabled { get; set; }
    }

    public class SaveSystemConfigCommandHandler : IRequestHandler<SaveSystemConfigCommand, MethodResult<bool>>
    {
        private readonly ISystemConfigRepository _systemConfigRepository;

        public SaveSystemConfigCommandHandler(ISystemConfigRepository systemConfigRepository)
        {
            _systemConfigRepository = systemConfigRepository;
        }

        public async Task<MethodResult<bool>> Handle(SaveSystemConfigCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var systemConfig = await _systemConfigRepository.Queryable.FirstOrDefaultAsync(cancellationToken);
            if (systemConfig == null)
            {
                systemConfig = new SystemConfig
                {
                    Type = "HomeWorkExtra",
                    IsEnabled = request.IsEnabled
                };
                _systemConfigRepository.Add(systemConfig);
            }
            else
            {
                systemConfig.IsEnabled = request.IsEnabled;
                _systemConfigRepository.Update(systemConfig);
            }
            await _systemConfigRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            methodResult.Result = true;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
