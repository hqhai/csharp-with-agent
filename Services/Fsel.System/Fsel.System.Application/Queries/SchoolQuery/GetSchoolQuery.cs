// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.SchoolQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetSchoolQuery : IRequest<MethodResult<IList<SchoolModel>>>
    {
        public IList<Guid>? ProvinceIds { get; set; }
        public IList<Guid>? DistrictIds { get; set; }
        public IList<Guid>? SchoolIds { get; set; }
    }

    public class GetSchoolQueryHandler : IRequestHandler<GetSchoolQuery, MethodResult<IList<SchoolModel>>>
    {
        private readonly ISchoolRepository _schoolRepository;
        private readonly IMapper _mapper;

        public GetSchoolQueryHandler(ISchoolRepository schoolRepository, IMapper mapper)
        {
            _schoolRepository = schoolRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<SchoolModel>>> Handle(GetSchoolQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<SchoolModel>> methodResult = new MethodResult<IList<SchoolModel>>();

            var query = _schoolRepository.Queryable.Include(x => x.Location).AsQueryable();

            if (request.ProvinceIds != null && request.ProvinceIds.Any())
            {
                query = query.Where(x => x.Location != null && x.Location.ParentId.HasValue && request.ProvinceIds.Contains(x.Location.ParentId.Value));
            }

            if (request.SchoolIds != null && request.SchoolIds.Any())
            {
                query = query.Where(x => request.SchoolIds.Contains(x.Id));
            }

            if (request.SchoolIds != null && request.SchoolIds.Any())
            {
                query = query.Where(x => request.SchoolIds.Contains(x.Id));
            }
            var schools = await query.ToListAsync(cancellationToken);
            methodResult.Result = _mapper.Map(schools, methodResult.Result);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
