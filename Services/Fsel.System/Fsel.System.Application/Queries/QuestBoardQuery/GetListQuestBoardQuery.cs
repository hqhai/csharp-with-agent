// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.QuestBoardQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System;
    using global::System.Collections.Generic;
    using global::System.Linq;
    using global::System.Text;
    using global::System.Threading.Tasks;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetListQuestBoardQuery : IRequest<MethodResult<List<QuestBoardModel>>>
    {
        public Guid PackageId { get; set; }
        public EnumQuestBoardCategory CategoryBoard { get; set; }

    }

    public class GetListQuestBoardQueryHandler : IRequestHandler<GetListQuestBoardQuery, MethodResult<List<QuestBoardModel>>>
    {
        private readonly IQuestBoardRepository _questBoardRepository;
        private readonly IMapper _mapper;
        public GetListQuestBoardQueryHandler(IQuestBoardRepository questBoardRepository, IMapper mapper)
        {
            _questBoardRepository = questBoardRepository;
            _mapper = mapper;
        }
        public async Task<MethodResult<List<QuestBoardModel>>> Handle(GetListQuestBoardQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<List<QuestBoardModel>>();
            var questBoard = _questBoardRepository.Queryable
     .Where(x => x.Category == request.CategoryBoard )
     .ToList();
            methodResult.Result = _mapper.Map<List<QuestBoardModel>>(questBoard);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
