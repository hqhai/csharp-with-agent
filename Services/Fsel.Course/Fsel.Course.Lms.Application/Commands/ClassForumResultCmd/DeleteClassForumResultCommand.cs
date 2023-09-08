// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.ClassForumResultCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class DeleteClassForumResultCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }

    public class DeleteClassForumResultCommandHandler : IRequestHandler<DeleteClassForumResultCommand, MethodResult<bool>>
    {
        private readonly IClassForumResultRepository _classForumResulRepository;

        public DeleteClassForumResultCommandHandler(IClassForumResultRepository classForumResulRepository)
        {
            _classForumResulRepository = classForumResulRepository;
        }

        public async Task<MethodResult<bool>> Handle(DeleteClassForumResultCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var classForumResult = await _classForumResulRepository.GetIncludeByIdAsync(request.Id);

            if (classForumResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassForumResultErrorCode.ClassForumResultNotExist), nameof(request.Id), request.Id);
                return methodResult;
            }
            if (classForumResult.Status != EnumClassForumResultStatus.PendingForGrading && classForumResult.Status != EnumClassForumResultStatus.Graded)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassForumResultErrorCode.CanNotDeleteInCurrentStatus), nameof(classForumResult.Status), classForumResult.Status);
                return methodResult;
            }
            await _classForumResulRepository.ExecuteTransactionAsync(async () =>
        {
            var result = await _classForumResulRepository.DeleteAsync(classForumResult);
            await _classForumResulRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = result;
            return methodResult;
        });

            return methodResult;
        }
    }
}
