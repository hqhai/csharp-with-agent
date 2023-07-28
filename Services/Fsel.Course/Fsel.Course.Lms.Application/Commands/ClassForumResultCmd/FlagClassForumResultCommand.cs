// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.ClassForumResultCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class FlagClassForumResultCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }

    public class FlagClassForumResultCommandHandler : IRequestHandler<FlagClassForumResultCommand, MethodResult<bool>>
    {
        private readonly IClassForumResultRepository _classForumResultRepository;

        public FlagClassForumResultCommandHandler(IClassForumResultRepository classForumResultRepository)
        {
            _classForumResultRepository = classForumResultRepository;
        }

        public async Task<MethodResult<bool>> Handle(FlagClassForumResultCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            MethodResult<bool> methodResult = new MethodResult<bool>();
            var classForumResult = await _classForumResultRepository.Queryable
                            .Where(e => e.Id == request.Id)
                            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
            if (classForumResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Id));
                return methodResult;
            }
            if (classForumResult.Status != EnumClassForumResultStatus.Graded)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassForumResultErrorCode.ClassForumResultStatusNotPendding));
                return methodResult;
            }
            await _classForumResultRepository.ExecuteTransactionAsync(async () =>
            {
                classForumResult.IsFlagged = true;
                classForumResult = _classForumResultRepository.Update(classForumResult);

                await _classForumResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = true;
                return methodResult;
            });
            return methodResult;
        }
    }
}
