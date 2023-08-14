// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.QuestBoardQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetQuestBoardQueryByAdminQuery : IRequest<MethodResult<QuestBoardModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetQuestBoardQueryByAdminQueryHandler : IRequestHandler<GetQuestBoardQueryByAdminQuery, MethodResult<QuestBoardModel>>
    {
        private readonly IQuestBoardRepository _questBoardRepository;
        private readonly IMapper _mapper;

        public GetQuestBoardQueryByAdminQueryHandler(IQuestBoardRepository questBoardRepository, IMapper mapper)
        {
            _questBoardRepository = questBoardRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<QuestBoardModel>> Handle(GetQuestBoardQueryByAdminQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<QuestBoardModel>();
            var questBoard = await _questBoardRepository.Queryable.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (questBoard == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(questBoard));
                return methodResult;
            }
            methodResult.Result = _mapper.Map<QuestBoardModel>(questBoard);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
