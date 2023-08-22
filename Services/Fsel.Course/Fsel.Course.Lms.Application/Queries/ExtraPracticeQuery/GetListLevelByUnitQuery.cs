// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ExtraPracticeQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetListLevelByUnitQuery : IRequest<MethodResult<object>>
    {
    }

    public class GetListLevelByUnitQueryHandler : IRequestHandler<GetListLevelByUnitQuery, MethodResult<object>>
    {
        private readonly IUnitRepository _unitRepository;

        public GetListLevelByUnitQueryHandler(IUnitRepository unitRepository)
        {
            _unitRepository = unitRepository;
        }

        public async Task<MethodResult<object>> Handle(GetListLevelByUnitQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<object> methodResult = new MethodResult<object>();
            var units = await _unitRepository.Queryable.OrderBy(x => x.CreatedDate).GroupBy(x => x.CourseLevel).Select(x => new
            {
                CourseLevel = x.Key,
                Units = x.Select(y => new
                {
                    Id = y.Id,
                    Name = y.Name,
                    Code = y.Code
                }).ToList()
            }).ToListAsync(cancellationToken);

            methodResult.Result = units;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
