// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.HomeWorkConfigCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.HomeWorkConfigs;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class DeleteHomeWorkConfigsCommand : DeleteHomeWorkConfigsCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class DeleteHomeWorkConfigsCommandHandler : IRequestHandler<DeleteHomeWorkConfigsCommand, MethodResult<bool>>
    {
        private readonly IHomeWorkConfigRepository _homeWorkConfigRepository;

        public DeleteHomeWorkConfigsCommandHandler(IHomeWorkConfigRepository homeWorkConfigRepository)
        {
            _homeWorkConfigRepository = homeWorkConfigRepository;
        }

        public async Task<MethodResult<bool>> Handle(DeleteHomeWorkConfigsCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var homeWorkConfigs = await _homeWorkConfigRepository.Queryable.WhereBulkContains(request.HomeWorkConfigIds, p => p.Id).ToListAsync(cancellationToken);

            if (homeWorkConfigs.Any())
            {
                await _homeWorkConfigRepository.ExecuteTransactionAsync(async () =>
                {
                    await _homeWorkConfigRepository.DeleteListAsync(homeWorkConfigs);
                    methodResult.StatusCode = StatusCodes.Status200OK;
                    methodResult.Result = true;
                    return methodResult;
                });
            }
            return methodResult;
        }
    }
}
