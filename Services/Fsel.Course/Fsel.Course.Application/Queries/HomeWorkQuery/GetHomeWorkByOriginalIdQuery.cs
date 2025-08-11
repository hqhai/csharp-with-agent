// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.HomeWorkQuery
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetHomeWorkByOriginalIdQuery : IRequest<MethodResult<HomeWorkModel>>
    {
        public Guid OriginalId { get; set; }
    }

    public class GetHomeWorkByOriginalIdQueryHandler : IRequestHandler<GetHomeWorkByOriginalIdQuery, MethodResult<HomeWorkModel>>
    {
        private readonly IHomeWorkRepository _homeWorkRepository;
        private readonly IMapper _mapper;

        public GetHomeWorkByOriginalIdQueryHandler(IHomeWorkRepository homeWorkRepository,
                                                IMapper mapper)
        {
            _homeWorkRepository = homeWorkRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<HomeWorkModel>> Handle(GetHomeWorkByOriginalIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<HomeWorkModel> methodResult = new MethodResult<HomeWorkModel>();

            var homeWork = await _homeWorkRepository.Queryable
                                                    .Where(x => x.OriginalId == request.OriginalId && x.VersionStatus == Common.Enums.EnumVersionStatus.LastVersion)
                                                    .AsNoTracking()
                                                    .FirstOrDefaultAsync(cancellationToken: cancellationToken);
            if (homeWork == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(homeWork));
                return methodResult;
            }

            methodResult.Result = _mapper.Map<HomeWorkModel>(homeWork);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
