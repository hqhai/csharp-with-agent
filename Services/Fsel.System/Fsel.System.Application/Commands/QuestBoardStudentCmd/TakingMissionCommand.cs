// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.QuestBoardStudentCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.System.Application.Services.UserServices;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class TakingMissionCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }

    public class TakingMissionCommandHandler : IRequestHandler<TakingMissionCommand, MethodResult<bool>>
    {
        private readonly IUserService _userService;
        private readonly IQuestBoardRepository _questBoardRepository;
        private readonly IQuestBoardStudentRepository _questBoardStudentRepository;
        private readonly AuthContext _authContext;

        public TakingMissionCommandHandler(IUserService userService
            , IQuestBoardRepository questBoardRepository
            , IQuestBoardStudentRepository questBoardStudentRepository
            , AuthContext authContext)
        {
            _userService = userService;
            _questBoardRepository = questBoardRepository;
            _questBoardStudentRepository = questBoardStudentRepository;
            _authContext = authContext;
        }

        public async Task<MethodResult<bool>> Handle(TakingMissionCommand request, CancellationToken cancellationToken)
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
            if (questBoardStudent != null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(questBoardStudent));
                return methodResult;
            }
            questBoardStudent = new QuestBoardStudent
            {
                QuestBoardId = request.Id,
                StudentId = student.Id,
                Status = EnumQuestBoardStatus.Process
            };
            await _questBoardStudentRepository.ExecuteTransactionAsync(async () =>
            {
                questBoardStudent = _questBoardStudentRepository.Add(questBoardStudent);
                await _questBoardRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }
    }
}
