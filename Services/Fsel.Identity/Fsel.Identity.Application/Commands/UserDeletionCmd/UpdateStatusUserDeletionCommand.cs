// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.UserDeletionCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Identity.Domain.Enums;
    using Fsel.Identity.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateStatusUserDeletionCommand : IRequest<MethodResult<bool>>
    {
        public EnumUserDeletionStatus Status { get; set; }
        public Guid? UserId { get; set; }
    }

    public class UpdateStatusUserDeletionCommandHandler : IRequestHandler<UpdateStatusUserDeletionCommand, MethodResult<bool>>
    {
        private readonly IUserDeletionRepository _userDeletionRepository;
        private readonly AuthContext _authContext;
        private readonly IMediator _mediator;

        public UpdateStatusUserDeletionCommandHandler(IUserDeletionRepository userDeletionRepository,
            AuthContext authContext,
            IMediator mediator)
        {
            _userDeletionRepository = userDeletionRepository;
            _authContext = authContext;
            _mediator = mediator;
        }

        public async Task<MethodResult<bool>> Handle(UpdateStatusUserDeletionCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();
            var userId = request.UserId ?? _authContext.CurrentUserId;
            var userDeletion = await _userDeletionRepository.Queryable.Where(x => x.UserId == userId && x.Status == EnumUserDeletionStatus.New)
                                                            .FirstOrDefaultAsync(cancellationToken);
            if (userDeletion == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(userDeletion));
                return methodResult;
            }
            userDeletion.Status = request.Status;
            if (request.Status == EnumUserDeletionStatus.Deleted)
            {
                if (userDeletion.DeletionDate > DateTime.UtcNow)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(userDeletion.DeletionDate));
                    return methodResult;
                }
                var deleteAccountResult = await _mediator.Send(new AdminCmd.DeleteUserCommand { Id = userId, IsHashDelete = true }, cancellationToken);
                if (!deleteAccountResult.IsOK)
                {
                    methodResult.AddError(deleteAccountResult.ErrorMessages);
                    return methodResult;
                }
            }

            _userDeletionRepository.Update(userDeletion);
            await _userDeletionRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

            methodResult.Result = true;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
