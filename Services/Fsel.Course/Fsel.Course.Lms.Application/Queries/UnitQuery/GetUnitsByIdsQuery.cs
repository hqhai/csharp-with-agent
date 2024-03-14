// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.UnitQuery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetUnitsByIdsQuery : IRequest<MethodResult<IList<UnitModel>>>
    {
        public IList<Guid>? UnitIds { get; set; }
    }

    public class GetUnitsByIdsQueryHandler : IRequestHandler<GetUnitsByIdsQuery, MethodResult<IList<UnitModel>>>
    {
        private readonly IUnitRepository _unitRepository;

        public GetUnitsByIdsQueryHandler(IUnitRepository unitRepository)
        {
            _unitRepository = unitRepository;
        }

        public async Task<MethodResult<IList<UnitModel>>> Handle(GetUnitsByIdsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<UnitModel>> methodResult = new MethodResult<IList<UnitModel>>();

            if (request.UnitIds == null || request.UnitIds.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.UnitIds));
                return methodResult;
            }

            var units = await _unitRepository.Queryable.Where(p => request.UnitIds.Contains(p.Id)).Select(x => new UnitModel
            {
                Id = x.Id,
                Name = x.Name,
                CourseLevel = x.CourseLevel,
                IsActive = x.IsArchive,
            }).ToListAsync(cancellationToken);

            methodResult.Result = units;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
