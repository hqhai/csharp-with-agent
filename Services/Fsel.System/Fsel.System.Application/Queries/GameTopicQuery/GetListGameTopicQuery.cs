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

    public class GetListGameTopicQuery : BaseQueryModel, IRequest<MethodResult<IList<GameTopicModel>>>
    {
        public string? Value { get; set; }

        public EnumCourseLevel? CourseLevel { get; set; }
    }

    public class GetListGameTopicQueryHandler : IRequestHandler<GetListGameTopicQuery, MethodResult<IList<GameTopicModel>>>
    {
        private readonly IGameTopicRepository _gameTopicRepository;
        private readonly IMapper _mapper;

        public GetListGameTopicQueryHandler(IGameTopicRepository gameTopicRepository, IMapper mapper)
        {
            _gameTopicRepository = gameTopicRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<GameTopicModel>>> Handle(GetListGameTopicQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<GameTopicModel>> methodResult = new MethodResult<IList<GameTopicModel>>();

            var gameTopic = await _gameTopicRepository.Queryable
                        .Select(x => new GameTopicModel
                        {
                            Id = x.Id,
                            CreatedDate = x.CreatedDate,
                            CreatedFullName = x.CreatedFullName,
                            CourseLevel = x.CourseLevel,
                            Skill = x.Skill,
                            UnitOrder = x.UnitOrder,
                            Value = x.Value,
                        }).ToListAsync(cancellationToken);

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                gameTopic = gameTopic.Where(m => m.Id.ToString() == request.Keyword || (m.Value ?? string.Empty).Contains(request.Keyword, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (request.CourseLevel != null)
            {
                gameTopic = gameTopic.Where(x => x.CourseLevel == request.CourseLevel).ToList();
            }
            methodResult.Result = gameTopic;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
