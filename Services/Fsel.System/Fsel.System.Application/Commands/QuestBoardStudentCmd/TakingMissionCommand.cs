// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.QuestBoardStudentCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class TakingMissionCommand : IRequest<MethodResult<QuestBoardModel>>
    {
        public Guid Id { get; set; }
    }

    public class TakingMissionCommandHandler : IRequestHandler<TakingMissionCommand, MethodResult<QuestBoardModel>>
    {
        private readonly IMapper _mapper;
        private readonly IQuestBoardRepository _questBoardRepository;

        public TakingMissionCommandHandler(IMapper mapper
            , IQuestBoardRepository questBoardRepository)
        {
            _mapper = mapper;
            _questBoardRepository = questBoardRepository;
        }

        public async Task<MethodResult<QuestBoardModel>> Handle(TakingMissionCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<QuestBoardModel>();

            var questBoard = await _questBoardRepository.GetByIdAsync(request.Id);
            if (questBoard == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(questBoard));
                return methodResult;
            }
            await _questBoardRepository.ExecuteTransactionAsync(async () =>
            {
                questBoard = _questBoardRepository.Update(questBoard);
                await _questBoardRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<QuestBoardModel>(questBoard);
                return methodResult;
            });

            return methodResult;
        }
    }
}
