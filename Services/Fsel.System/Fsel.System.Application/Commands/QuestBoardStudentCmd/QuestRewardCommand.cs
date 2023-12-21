// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.QuestBoardStudentCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.System.Application.Services.UserServices;
    using Fsel.System.Domain.Enums.ErrorCodes;
    using Fsel.System.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class QuestRewardCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }

    public class QuestRewardCommandHandler : IRequestHandler<QuestRewardCommand, MethodResult<bool>>
    {
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IQuestBoardStudentRepository _questBoardStudentRepository;
        private readonly IQuestBoardRepository _questBoardRepository;

        public QuestRewardCommandHandler(AuthContext authContext
            , IUserService userService
            , IQuestBoardStudentRepository questBoardStudentRepository
            , IQuestBoardRepository questBoardRepository)
        {
            _authContext = authContext;
            _userService = userService;
            _questBoardStudentRepository = questBoardStudentRepository;
            _questBoardRepository = questBoardRepository;
        }

        public async Task<MethodResult<bool>> Handle(QuestRewardCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();
            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }
            var student = studentResult?.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }
            var questBoard = await _questBoardRepository.GetByIdAsync(request.Id);
            if (questBoard == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(questBoard));
                return methodResult;
            }
            var questBoardStudent = await _questBoardStudentRepository.Queryable.FirstOrDefaultAsync(x => x.QuestBoardId == request.Id && x.StudentId == student.Id, cancellationToken);
            if (questBoardStudent == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(questBoardStudent));
                return methodResult;
            }
            if (questBoardStudent.Status != EnumQuestBoardStudentStatus.Done)
            {
                methodResult.AddErrorBadRequest(nameof(EnumQuestBoardErrorcode.QuestBoardStudentStatusDone), nameof(questBoardStudent));
                return methodResult;
            }
           // questBoardStudent.Status = EnumQuestBoardStudentStatus.Completed;
            var updateTokenStudent = await _userService.UpdateStudentByTokenAsync(new Services.UserServices.Models.UpdateStudentByTokenModel { NumberOfToken = questBoard.NumberOfStars, StudentId = student.Id });
            if (!updateTokenStudent.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(updateTokenStudent));
                return methodResult;
            }
            await _questBoardStudentRepository.ExecuteTransactionAsync(async () =>
            {
                questBoardStudent = _questBoardStudentRepository.Update(questBoardStudent);
                await _questBoardStudentRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }
    }
}
