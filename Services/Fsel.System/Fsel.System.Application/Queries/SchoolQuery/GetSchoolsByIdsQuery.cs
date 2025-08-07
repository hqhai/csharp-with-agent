// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.SchoolQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetSchoolsByIdsQuery : IRequest<MethodResult<IList<SchoolModel>>>
    {
        public EnumEducationLevel? EducationLevel { get; set; }
        public IList<Guid>? Ids { get; set; }
    }

    public class GetSchoolsByIdsQueryHandler : IRequestHandler<GetSchoolsByIdsQuery, MethodResult<IList<SchoolModel>>>
    {
        private readonly ISchoolRepository _schoolRepository;
        private readonly IMapper _mapper;
        private readonly ICrmLocationRepository _locationCrmRepository;

        public GetSchoolsByIdsQueryHandler(ISchoolRepository schoolRepository, IMapper mapper, ICrmLocationRepository locationCrmRepository)
        {
            _schoolRepository = schoolRepository;
            _mapper = mapper;
            _locationCrmRepository = locationCrmRepository;
        }

        public async Task<MethodResult<IList<SchoolModel>>> Handle(GetSchoolsByIdsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<SchoolModel>> methodResult = new MethodResult<IList<SchoolModel>>();
            if (request.Ids == null || !request.Ids.Any())
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var query = _locationCrmRepository.Queryable.Where(p => request.Ids.Contains(p.GlobalId) && p.TypeName == EnumCrmLocationTypeName.School);
            if (request.EducationLevel.HasValue)
            {
                var educationLevel = (EnumCrmLocationTypeLevel)request.EducationLevel.Value;
                query = query.Where(m => m.TypeLevel == educationLevel);
            }
            var schools = await query.ToListAsync(cancellationToken);

            methodResult.Result = schools.Select(p => new SchoolModel
            {
                Id = p.GlobalId,
                Name = p.Name,
                LongPath = p.LongPath,
                ShortPath = p.ShortPath,
                IdPath = p.IdPath,
                EducationLevel = Enum.IsDefined(typeof(EnumEducationLevel), (int?)p.TypeLevel ?? default) ? (EnumEducationLevel?)p.TypeLevel : null,
                LocalId = p.LocalId,
            }).ToList();
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
