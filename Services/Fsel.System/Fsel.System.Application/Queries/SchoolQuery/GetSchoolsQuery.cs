// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.SchoolQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetSchoolsQuery : IRequest<MethodResult<IList<SchoolModel>>>
    {
        public EnumEducationLevel? EducationLevel { get; set; }
        public IList<Guid>? SchoolIds { get; set; }
    }

    public class GetSchoolsQueryHandler : IRequestHandler<GetSchoolsQuery, MethodResult<IList<SchoolModel>>>
    {
        private readonly ICrmLocationRepository _crmLocationRepository;

        public GetSchoolsQueryHandler(ICrmLocationRepository crmLocationRepository)
        {
            _crmLocationRepository = crmLocationRepository;
        }

        public async Task<MethodResult<IList<SchoolModel>>> Handle(GetSchoolsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<SchoolModel>>();

            var query = _crmLocationRepository.Queryable.Where(p => p.TypeName == EnumCrmLocationTypeName.School);

            if (request.EducationLevel.HasValue)
            {
                var educationLevel = (EnumCrmLocationTypeLevel)request.EducationLevel.Value;
                query = query.Where(m => m.TypeLevel == educationLevel);
            }
            if (request.SchoolIds != null && request.SchoolIds.Any())
            {
                query = query.Where(m => request.SchoolIds.Contains(m.GlobalId));
            }

            var list = await query.Select(p => new SchoolModel
            {
                Id = p.GlobalId,
                Name = p.Name,
                LongPath = p.LongPath,
                ShortPath = p.ShortPath,
                IdPath = p.IdPath,
            }).ToListAsync(cancellationToken: cancellationToken);
            methodResult.Result = list;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
