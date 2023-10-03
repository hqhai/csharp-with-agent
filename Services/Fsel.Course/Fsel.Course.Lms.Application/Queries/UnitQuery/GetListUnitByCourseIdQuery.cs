// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.UnitQuery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetListUnitByCourseIdQuery : BaseQueryModel, IRequest<MethodResult<IList<UnitModel>>>
    {
        public Guid CourseId { get; set; }
    }

    public class GetListUnitByCourseIdQueryHandler : IRequestHandler<GetListUnitByCourseIdQuery, MethodResult<IList<UnitModel>>>
    {
        private readonly IUnitRepository _unitRepository;

        public GetListUnitByCourseIdQueryHandler(IUnitRepository unitRepository)
        {
            _unitRepository = unitRepository;
        }

        public async Task<MethodResult<IList<UnitModel>>> Handle(GetListUnitByCourseIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<UnitModel>>();

            var units = await _unitRepository.Queryable
                            .Include(x => x.CourseUnitMockTests)
                            .Where(x => x.CourseUnitMockTests.Select(x => x.CourseId).FirstOrDefault() == request.CourseId)
                            .Select(x => new UnitModel
                            {
                                Id = x.Id,
                                CreatedDate = x.CreatedDate,
                                DisplayOrder = x.CourseUnitMockTests.Select(x => x.Number).FirstOrDefault(),
                            }).ApplySort(request).ToListAsync(cancellationToken);
            methodResult.Result = units;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
