// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.SchoolQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetSchoolsByIdsQuery : IRequest<MethodResult<IList<SchoolModel>>>
    {
        public IList<Guid>? Ids { get; set; }
    }

    public class GetSchoolsByIdsQueryHandler : IRequestHandler<GetSchoolsByIdsQuery, MethodResult<IList<SchoolModel>>>
    {
        private readonly ISchoolRepository _schoolRepository;
        private readonly IMapper _mapper;

        public GetSchoolsByIdsQueryHandler(ISchoolRepository schoolRepository, IMapper mapper)
        {
            _schoolRepository = schoolRepository;
            _mapper = mapper;
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

            var schools = await _schoolRepository.GetByIdsAsync(request.Ids);
            if (schools == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(schools));
                return methodResult;
            }
            methodResult.Result = _mapper.Map<IList<SchoolModel>>(schools);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
