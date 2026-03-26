// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.LevelQuery
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetLevelsQuery : IRequest<MethodResult<List<LevelModel>>>
    {
        public List<Guid> LevelIds { get; set; }
    }

    public class GetLevelsQueryHandler : IRequestHandler<GetLevelsQuery, MethodResult<List<LevelModel>>>
    {
        private readonly IMapper _mapper;
        private readonly ILevelRepository _levelRepository;

        public GetLevelsQueryHandler(IMapper mapper, ILevelRepository levelRepository)
        {
            _mapper = mapper;
            _levelRepository = levelRepository;
        }

        public async Task<MethodResult<List<LevelModel>>> Handle(GetLevelsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<List<LevelModel>>();

            var levels = await _levelRepository.ReadQueryable.WhereBulkContains(request.LevelIds, x => x.Id).ToListAsync(cancellationToken);

            methodResult.Result = levels.Select(x => _mapper.Map<LevelModel>(x)).ToList();
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
