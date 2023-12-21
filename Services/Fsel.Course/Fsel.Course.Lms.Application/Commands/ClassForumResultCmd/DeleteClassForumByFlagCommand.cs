// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.ClassForumResultCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class DeleteClassForumByFlagCommand : IRequest<VoidMethodResult>
    {
        public IList<Guid>? Ids { get; set; }
    }

    public class DeleteClassForumFlaggedCommandHandler : IRequestHandler<DeleteClassForumByFlagCommand, VoidMethodResult>
    {
        private readonly IClassForumResultRepository _classForumResultRepository;

        public DeleteClassForumFlaggedCommandHandler(IClassForumResultRepository classForumResultRepository)
        {
            _classForumResultRepository = classForumResultRepository;
        }

        public async Task<VoidMethodResult> Handle(DeleteClassForumByFlagCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            VoidMethodResult methodResult = new VoidMethodResult();

            var classForumResult = await _classForumResultRepository.Queryable.Where(x => request.Ids!.Contains(x.Id)).ToListAsync(cancellationToken);

            await _classForumResultRepository.DeleteListAsync(classForumResult);
            await _classForumResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
