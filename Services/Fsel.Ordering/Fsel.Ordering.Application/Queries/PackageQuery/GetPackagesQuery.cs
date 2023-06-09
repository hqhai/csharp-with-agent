// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.PackageQuery
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetPackagesQuery : IRequest<MethodResult<List<PackageModel>>>
    {
    }

    public class GetPackagesQueryHandler : IRequestHandler<GetPackagesQuery, MethodResult<List<PackageModel>>>
    {
        private readonly IPackageRepository _packageRepository;
        private readonly IMapper _mapper;

        public GetPackagesQueryHandler(IPackageRepository packageRepository, IMapper mapper)
        {
            _packageRepository = packageRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<List<PackageModel>>> Handle(GetPackagesQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<List<PackageModel>> methodResult = new MethodResult<List<PackageModel>>();

            var packages = await _packageRepository.Queryable.ToListAsync(cancellationToken);
            methodResult.Result = _mapper.Map<List<PackageModel>>(packages);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
