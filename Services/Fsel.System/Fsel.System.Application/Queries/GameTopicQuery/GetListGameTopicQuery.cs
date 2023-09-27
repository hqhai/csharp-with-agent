// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.GameTopicQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System;
    using global::System.Linq;
    using global::System.Threading.Tasks;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetListGameTopicQuery : BaseQueryModel, IRequest<MethodResult<IList<GameTopicsModel>>>
    {
        public string? Value { get; set; }

        public EnumGameCourseLevel? CourseLevel { get; set; }
    }

    public class GetListGameTopicQueryHandler : IRequestHandler<GetListGameTopicQuery, MethodResult<IList<GameTopicsModel>>>
    {
        private readonly IGameTopicRepository _gameTopicRepository;
        private readonly IMapper _mapper;

        public GetListGameTopicQueryHandler(IGameTopicRepository gameTopicRepository, IMapper mapper)
        {
            _gameTopicRepository = gameTopicRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<GameTopicsModel>>> Handle(GetListGameTopicQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<GameTopicsModel>> methodResult = new MethodResult<IList<GameTopicsModel>>();

            var query = _gameTopicRepository.Queryable;

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                query = query.Where(m => m.Id.ToString() == request.Keyword || (m.Value ?? string.Empty).Contains(request.Keyword, StringComparison.OrdinalIgnoreCase));
            }

            if (request.CourseLevel != null)
            {
                query = query.Where(x => x.CourseLevel == request.CourseLevel);
            }

            var gameTopics = await query.GroupBy(x => x.UnitOrder)
                        .Select(x => new GameTopicsModel
                        {
                            UnitOrder = x.Key,
                            GameTopics = x.Select(x => new GameTopicModel
                            {
                                Id = x.Id,
                                CreatedDate = x.CreatedDate,
                                CreatedFullName = x.CreatedFullName,
                                CourseLevel = x.CourseLevel,
                                Skill = x.Skill,
                                UnitOrder = x.UnitOrder,
                                Value = x.Value,
                            }).ToList(),
                        }).ToListAsync(cancellationToken);

            methodResult.Result = gameTopics;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
