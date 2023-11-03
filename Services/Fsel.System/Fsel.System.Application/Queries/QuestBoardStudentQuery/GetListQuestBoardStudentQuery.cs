// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.QuestBoardStudentQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.System.Application.Queries.QuestBoardQuery;
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
    using static global::System.Runtime.InteropServices.JavaScript.JSType;

    public class GetListQuestBoardStudentQuery : IRequest<MethodResult<List<QuestBoardStudentModel>>>
    {
        public Guid? QuestBoardId { get; set; }

        public Guid? StudentId { get; set; }
    }
    public class GetListQuestBoardStudentQueryHandler : IRequestHandler<GetListQuestBoardStudentQuery, MethodResult<List<QuestBoardStudentModel>>>
    {
        private readonly IQuestBoardStudentRepository _questBoardStudentRepository;
        private readonly IMapper _mapper;

        public GetListQuestBoardStudentQueryHandler(IQuestBoardStudentRepository questBoardStudentRepository, IMapper mapper)
        {
            _questBoardStudentRepository = questBoardStudentRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<List<QuestBoardStudentModel>>> Handle(GetListQuestBoardStudentQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<List<QuestBoardStudentModel>>();
            var questBoardStudents =await _questBoardStudentRepository.Queryable.Where(x => x.StudentId == request.StudentId && x.QuestBoardId==request.QuestBoardId).Select(o=> new QuestBoardStudentModel
            {
                Id= o.Id,
                Status =o.Status,
                AchievedPoints = o.AchievedPoints,
                QuestBoard=o.QuestBoard,
                QuestBoardId = o.QuestBoardId,
                StudentId  =o.StudentId,
                ObjectId = o.ObjectId
            }).ToListAsync(cancellationToken);
            methodResult.Result = questBoardStudents;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }


}
