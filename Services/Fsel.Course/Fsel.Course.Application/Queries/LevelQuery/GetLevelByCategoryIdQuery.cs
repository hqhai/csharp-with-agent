// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.LevelQuery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetLevelByCategoryIdQuery : IRequest<MethodResult<IList<LevelModel>>>
    {
        public Guid CategoryId { get; set; }
    }

    public class GetLevelByCategoryIdQueryHandler : IRequestHandler<GetLevelByCategoryIdQuery, MethodResult<IList<LevelModel>>>
    {
        private readonly ILevelRepository _levelRepository;
        private readonly IMapper _mapper;

        public GetLevelByCategoryIdQueryHandler(ILevelRepository levelRepository,
                                                IMapper mapper)
        {
            _levelRepository = levelRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<LevelModel>>> Handle(GetLevelByCategoryIdQuery request, CancellationToken cancellationToken)
        {

            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<LevelModel>> methodResult = new MethodResult<IList<LevelModel>>();

            var levels = await _levelRepository.Queryable
                                               .Include(x => x.Category)
                                               .Where(x => x.Category != null && x.Category.ParentId == request.CategoryId)
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
