// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.EventCmd
{
    using System.ComponentModel.DataAnnotations;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class RemoveUserFromEventCommand : IRequest<MethodResult<bool>>
    {
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid CompetitionEventId { get; set; }
    }

    public class RemoveUserFromEventCommandHandler : IRequestHandler<RemoveUserFromEventCommand, MethodResult<bool>>
    {
        private readonly IEventManagerRepository _eventManagerRepository;

        public RemoveUserFromEventCommandHandler(
            IEventManagerRepository eventManagerRepository)
        {
            _eventManagerRepository = eventManagerRepository;
        }

        public async Task<MethodResult<bool>> Handle(RemoveUserFromEventCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var methodResult = new MethodResult<bool>();

            // Tìm bản ghi sự kiện người dùng
            var eventManager = await _eventManagerRepository.Queryable
                .FirstOrDefaultAsync(x => x.UserId == request.UserId && x.CompetitionEventId == request.CompetitionEventId, cancellationToken)
                .ConfigureAwait(false);

            await _eventManagerRepository.ExecuteTransactionAsync(async () =>
            {
                if (eventManager == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), "User is not assigned to this event");
                    return methodResult;
                }

                // Xóa bản ghi
                await _eventManagerRepository.DeleteAsync(eventManager).ConfigureAwait(false);
                await _eventManagerRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.Result = true;
                return methodResult;
            });

            methodResult.Result = true;
            return methodResult;
        }
    }
}
