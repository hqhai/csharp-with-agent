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

    public class GetSchoolByIdQuery : IRequest<MethodResult<object>>
    {
        public Guid Id { get; set; }
    }

    public class GetSchoolByIdQueryHandler : IRequestHandler<GetSchoolByIdQuery, MethodResult<object>>
    {
        private readonly ISchoolRepository _schoolRepository;
        private readonly IMapper _mapper;

        public GetSchoolByIdQueryHandler(ISchoolRepository schoolRepository, IMapper mapper)
        {
            _schoolRepository = schoolRepository;
            _mapper = mapper;
        }
        public async Task<MethodResult<object>> Handle(GetSchoolByIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<object>> methodResult = new MethodResult<IList<object>>();

            var schools = await _schoolRepository.Queryable
                                                 .Where(x => x.Id == request.Id)
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
