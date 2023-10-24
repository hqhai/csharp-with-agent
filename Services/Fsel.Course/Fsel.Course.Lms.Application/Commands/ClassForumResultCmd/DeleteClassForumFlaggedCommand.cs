// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.ClassForumResultCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class DeleteClassForumFlaggedCommand : IRequest<VoidMethodResult>
    {
        public IList<Guid>? Ids { get; set; }
        public EnumFlagStatus? Status { get; set; }
        public EnumInteractionType? InteractionType { get; set; }
    }

    public class DeleteClassForumFlaggedCommandHandler : IRequestHandler<DeleteClassForumFlaggedCommand, VoidMethodResult>
    {
        private readonly IClassForumResultRepository _classForumResultRepository;

        public DeleteClassForumFlaggedCommandHandler(IClassForumResultRepository classForumResultRepository)
        {
            _classForumResultRepository = classForumResultRepository;
        }

        public async Task<VoidMethodResult> Handle(DeleteClassForumFlaggedCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            VoidMethodResult methodResult = new VoidMethodResult();

            if (request.Status == EnumFlagStatus.Approve && request.InteractionType == EnumInteractionType.ClassForum)
            {
                var classForumResult = await _classForumResultRepository.Queryable.Where(x => request.Ids!.Contains(x.Id)).ToListAsync(cancellationToken);

                await _classForumResultRepository.DeleteListAsync(classForumResult);
                await _classForumResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            }

            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
