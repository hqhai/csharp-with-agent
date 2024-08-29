// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.UserDeletionCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.Enums;
    using Fsel.Identity.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class DeleteUserToDeleteAccountCommand : IRequest<MethodResult<bool>>
    {
    }

    public class DeleteUserToDeleteAccountCommandHandler : IRequestHandler<DeleteUserToDeleteAccountCommand, MethodResult<bool>>
    {
        private readonly IUserDeletionRepository _userDeletionRepository;
        private readonly IMediator _mediator;

        public DeleteUserToDeleteAccountCommandHandler(IUserDeletionRepository userDeletionRepository,
            IMediator mediator)
        {
            _userDeletionRepository = userDeletionRepository;
            _mediator = mediator;
        }

        public async Task<MethodResult<bool>> Handle(DeleteUserToDeleteAccountCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();
            var userDeletions = await _userDeletionRepository.Queryable.Where(x => x.Status == EnumUserDeletionStatus.New && x.DeletionDate < DateTime.UtcNow)
                                                            .ToListAsync(cancellationToken);

            foreach (var userDeletion in userDeletions)
            {
                await _mediator.Publish(new UpdateStatusUserDeletionCommand { Status = EnumUserDeletionStatus.Deleted, UserId = userDeletion.UserId }, cancellationToken);
            }

            methodResult.Result = true;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
