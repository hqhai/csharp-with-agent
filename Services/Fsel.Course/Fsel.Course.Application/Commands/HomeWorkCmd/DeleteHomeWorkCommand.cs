// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.HomeWorkCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class DeleteHomeWorkCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }

    public class DeleteHomeWorkCommandHandler : IRequestHandler<DeleteHomeWorkCommand, MethodResult<bool>>
    {
        private readonly IHomeWorkRepository _homeWorkRepository;

        public DeleteHomeWorkCommandHandler(IHomeWorkRepository homeWorkRepository)
        {
            _homeWorkRepository = homeWorkRepository;
        }

        public async Task<MethodResult<bool>> Handle(DeleteHomeWorkCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var homeWork = await _homeWorkRepository.Queryable
                            .Include(x => x.HomeWorkQuestions)
                            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken: cancellationToken);

            if (homeWork == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(homeWork));
                return methodResult;
            }

            var isHomeWorkUsed = await _homeWorkRepository.IsHomeWorkUsed(request.Id);
            if (isHomeWorkUsed)
            {
                methodResult.AddErrorBadRequest(nameof(EnumHomeWorkErrorCode.HomeWorkUsed), nameof(request.Id), request.Id);
                return methodResult;
            }

            await _homeWorkRepository.ExecuteTransactionAsync(async () =>
            {
                var result = await _homeWorkRepository.DeleteAsync(homeWork);
                await _homeWorkRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = result;
                return methodResult;
            });

            return methodResult;
        }
    }
}
