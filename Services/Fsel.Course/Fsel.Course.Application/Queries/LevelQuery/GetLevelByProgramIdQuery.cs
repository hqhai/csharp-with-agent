// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.LevelQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetLevelByProgramIdQuery : IRequest<MethodResult<IList<LevelModel>>>
    {
        public Guid ProgramId { get; set; }
    }

    public class GetLevelByProgramIdQueryHandler : IRequestHandler<GetLevelByProgramIdQuery, MethodResult<IList<LevelModel>>>
    {
        private readonly ILevelRepository _levelRepository;
        private readonly IMapper _mapper;

        public GetLevelByProgramIdQueryHandler(ILevelRepository levelRepository,
                                               IMapper mapper)
        {
            _levelRepository = levelRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<LevelModel>>> Handle(GetLevelByProgramIdQuery request, CancellationToken cancellationToken)
        {

            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<LevelModel>> methodResult = new MethodResult<IList<LevelModel>>();

            var levels = await _levelRepository.Queryable
                                               .Where(x => x.ProgramId == request.ProgramId)
                                               .AsNoTracking()
                                               .ToListAsync(cancellationToken);
            if (levels == null || !levels.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(levels));
                return methodResult;
            }

            methodResult.Result = _mapper.Map<IList<LevelModel>>(levels);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
