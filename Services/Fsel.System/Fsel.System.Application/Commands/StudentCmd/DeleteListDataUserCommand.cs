// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.StudentCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.System.Application.Services.UserServices;
    using Fsel.System.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class DeleteListDataUserCommand : IRequest<MethodResult<bool>>
    {
        public Guid UserId { get; set; }
    }

    public class DeleteListDataUserCommandHandler : IRequestHandler<DeleteListDataUserCommand, MethodResult<bool>>
    {
        private readonly IQuestBoardStudentRepository _questBoardStudentRepository;
        private readonly IUserService _userService;

        public DeleteListDataUserCommandHandler(IQuestBoardStudentRepository questBoardStudentRepository, IUserService userService)
        {
            _questBoardStudentRepository = questBoardStudentRepository;
            _userService = userService;
        }
        public async Task<MethodResult<bool>> Handle(DeleteListDataUserCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var studentResult = await _userService.GetStudentByUserIdAsync(request.UserId);

            if (studentResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }

            var studentId = studentResult.Content?.Result?.Id;

            var questBoardStudent = await _questBoardStudentRepository.Queryable.Where(x => x.StudentId == studentId).ToListAsync(cancellationToken);

            await _questBoardStudentRepository.ExecuteTransactionAsync(async () =>
            {
                await _questBoardStudentRepository.DeleteListAsync(questBoardStudent);
                await _questBoardStudentRepository.UnitOfWork.SaveChangesAsync(true, false, cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }
    }
}
