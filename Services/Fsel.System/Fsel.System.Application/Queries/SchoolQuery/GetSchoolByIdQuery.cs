// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.SchoolQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.System.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetSchoolByIdsQuery : IRequest<MethodResult<IList<object>>>
    {
        public required IList<Guid> Ids { get; set; }
    }

    public class GetSchoolByIdsQueryHandler : IRequestHandler<GetSchoolByIdsQuery, MethodResult<IList<object>>>
    {
        private readonly ISchoolRepository _schoolRepository;
        private readonly IMapper _mapper;

        public GetSchoolByIdsQueryHandler(ISchoolRepository schoolRepository, IMapper mapper)
        {
            _schoolRepository = schoolRepository;
            _mapper = mapper;
        }
        public async Task<MethodResult<IList<object>>> Handle(GetSchoolByIdsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<object>> methodResult = new MethodResult<IList<object>>();

            var schools = await _schoolRepository.Queryable
                                                 .Include(x => x.Location)
                                                 .Where(x => request.Ids.Contains(x.Id))
                                                 .ToListAsync(cancellationToken);
            if (schools == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(schools));
                return methodResult;
            }

            methodResult.Result = _mapper.Map(schools, methodResult.Result);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
