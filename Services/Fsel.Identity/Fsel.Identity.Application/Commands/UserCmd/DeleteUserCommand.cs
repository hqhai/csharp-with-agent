// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.ActionResults;
using Fsel.Core.Base;
using MediatR;

namespace Fsel.Identity.Application.Commands.UserCmd
{
    public class DeleteUserCommand : IRequest<MethodResult<bool>>
    {
    }

    public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, MethodResult<bool>>
    {
        private readonly AuthContext _authContext;
        private readonly IMediator _mediator;

        public DeleteUserCommandHandler(AuthContext authContext, IMediator mediator)
        {
            _authContext = authContext;
            _mediator = mediator;
        }

        public async Task<MethodResult<bool>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            return await _mediator.Send(new AdminCmd.DeleteUserCommand { Id = _authContext.CurrentUserId }, cancellationToken);
        }
    }
}
