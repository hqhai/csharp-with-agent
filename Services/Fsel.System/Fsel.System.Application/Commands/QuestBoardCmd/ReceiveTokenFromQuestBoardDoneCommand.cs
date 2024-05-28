// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.QuestBoardCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Core.Extensions;
    using Fsel.Shared.Enums;
    using Fsel.System.Application.Services.UserServices;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.CommandModels.QuestBoards;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class ReceiveTokenFromQuestBoardDoneCommand : ReceiveTokenFromQuestBoardDoneCommandModel, IRequest<MethodResult<VoidMethodResult>>
    {
    }

    public class ReceiveTokenFromQuestBoardDoneCommandHandler : IRequestHandler<ReceiveTokenFromQuestBoardDoneCommand, MethodResult<VoidMethodResult>>
    {
        private readonly IQuestBoardOverallRepository _questBoardOverallRepository;
        private readonly IQuestBoardOverallStudentRepository _questBoardOverallStudentRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IQuestBoardRepository _questBoardRepository;
        private readonly IQuestBoardStudentRepository _questBoardStudentRepository;

        public ReceiveTokenFromQuestBoardDoneCommandHandler(IQuestBoardOverallRepository questBoardOverallRepository, IQuestBoardOverallStudentRepository questBoardOverallStudentRepository, AuthContext authContext, IUserService userService, IQuestBoardRepository questBoardRepository, IQuestBoardStudentRepository questBoardStudentRepository)
        {
            _questBoardOverallRepository = questBoardOverallRepository;
            _questBoardOverallStudentRepository = questBoardOverallStudentRepository;
            _authContext = authContext;
            _userService = userService;
            _questBoardRepository = questBoardRepository;
            _questBoardStudentRepository = questBoardStudentRepository;
        }

        public async Task<MethodResult<VoidMethodResult>> Handle(ReceiveTokenFromQuestBoardDoneCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<VoidMethodResult>();

            await ReceiveTokenFromQuestBoardOverall(request, _authContext.CurrentUserId, methodResult, cancellationToken);
            return methodResult;
        }

        private async Task ReceiveTokenFromQuestBoardOverall(ReceiveTokenFromQuestBoardDoneCommandModel request, Guid studentId, MethodResult<VoidMethodResult> methodResult, CancellationToken cancellationToken)
        {
            var questBoardOverall = await _questBoardOverallRepository.Queryable.FirstOrDefaultAsync(p => p.Id == request.QuestBoardOverallId, cancellationToken);

            if (questBoardOverall == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return;
            }

            var questBoardOverallStudent = await _questBoardOverallStudentRepository.Queryable.Where(p => p.QuestBoardOverallId == questBoardOverall.Id && p.StudentId == studentId && p.Status == EnumQuestBoardOverallStudentStatus.NotReceived).OrderBy(p => p.CreatedDate).FirstOrDefaultAsync(cancellationToken);

            if (questBoardOverallStudent == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return;
            }
            if (questBoardOverall.TargetValue > questBoardOverallStudent.CurrentValue)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Min));
                return;
            }
            if (questBoardOverallStudent.Status == EnumQuestBoardOverallStudentStatus.Received)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist));
                return;
            }

            await _questBoardOverallStudentRepository.ExecuteTransactionAsync(async () =>
            {
                var updateTokenStudent = await _userService.UpdateStudentByTokenAsync(new Services.UserServices.Models.UpdateStudentByTokenModel { NumberOfToken = questBoardOverall.Token, StudentId = questBoardOverallStudent.StudentId });
                if (!updateTokenStudent.IsSuccessStatusCode)
                {
                    methodResult.AddError(updateTokenStudent.Error);
                }
                questBoardOverallStudent.Status = EnumQuestBoardOverallStudentStatus.Received;
                await _questBoardOverallStudentRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                return methodResult;
            });
        }

        private async Task ReceiveTokenFromQuestBoard(ReceiveTokenFromQuestBoardDoneCommandModel request, Guid studentId, MethodResult<VoidMethodResult> methodResult, CancellationToken cancellationToken)
        {
            var questBoard = await _questBoardRepository.Queryable.FirstOrDefaultAsync(p => p.Id == request.QuestBoardId, cancellationToken);

            if (questBoard == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return;
            }

            var questBoardStudent = await _questBoardStudentRepository.Queryable.Where(p => p.QuestBoardId == questBoard.Id && p.StudentId == studentId && p.Status == EnumQuestBoardStudentStatus.NotReceived).OrderBy(p => p.CreatedDate).FirstOrDefaultAsync(cancellationToken);

            if (questBoardStudent == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return;
            }
            if (questBoard.TargetValue > questBoardStudent.CurrentValue)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Min));
                return;
            }
            if (questBoardStudent.Status == EnumQuestBoardStudentStatus.Received)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist));
                return;
            }

            await _questBoardStudentRepository.ExecuteTransactionAsync(async () =>
            {
                var updateTokenStudent = await _userService.UpdateStudentByTokenAsync(new Services.UserServices.Models.UpdateStudentByTokenModel { NumberOfToken = questBoard.Token, StudentId = questBoardStudent.StudentId });
                if (!updateTokenStudent.IsSuccessStatusCode)
                {
                    methodResult.AddError(updateTokenStudent.Error);
                }
                questBoardStudent.Status = EnumQuestBoardStudentStatus.Received;
                await _questBoardOverallStudentRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                return methodResult;
            });
        }
    }
}
