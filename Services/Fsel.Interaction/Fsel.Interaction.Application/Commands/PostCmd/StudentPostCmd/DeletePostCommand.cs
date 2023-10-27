// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Commands.PostCmd.StudentPostCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Interaction.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class DeletePostCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }

    public class DeletePostCommandHandler : IRequestHandler<DeletePostCommand, MethodResult<bool>>
    {
        private readonly IPostRepository _iPostRepository;

        public DeletePostCommandHandler(IPostRepository iPostRepository)
        {
            _iPostRepository = iPostRepository;
        }

        public async Task<MethodResult<bool>> Handle(DeletePostCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var studentPosts = await _iPostRepository.Queryable
                                    .Include(e => e.PostTags.Where(n => !n.IsDeleted))
                                    .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken: cancellationToken);

            if (studentPosts == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentPosts));
                return methodResult;
            }

            await _iPostRepository.ExecuteTransactionAsync(async () =>
            {
                var result = await _iPostRepository.DeleteAsync(studentPosts);
                await _iPostRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = result;
                return methodResult;
            });

            return methodResult;
        }
    }
}
