// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.GameTopicQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System.Collections.Generic;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetGameTopicsQuery : BaseQueryModel, IRequest<MethodResult<IList<GameTopicModel>>>
    {
        public EnumGameCourseLevel? CourseLevel { get; set; }
    }

    public class GetGameTopicsQueryHandler : IRequestHandler<GetGameTopicsQuery, MethodResult<IList<GameTopicModel>>>
    {
        private readonly IGameTopicRepository _gameTopicRepository;
        private readonly IMapper _mapper;

        public GetGameTopicsQueryHandler(IGameTopicRepository gameTopicRepository, IMapper mapper)
        {
            _gameTopicRepository = gameTopicRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<GameTopicModel>>> Handle(GetGameTopicsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<GameTopicModel>>();

            var query = _gameTopicRepository.Queryable;

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                query = query.Where(m => m.Id.ToString() == request.Keyword || (m.Value ?? string.Empty).ToLower().Trim().Contains(request.Keyword.ToLower().Trim(), StringComparison.OrdinalIgnoreCase));
            }

            if (request.CourseLevel != null)
            {
                query = query.Where(x => x.CourseLevel == request.CourseLevel);
            }

            methodResult.Result = _mapper.Map<IList<GameTopicModel>>(query);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
