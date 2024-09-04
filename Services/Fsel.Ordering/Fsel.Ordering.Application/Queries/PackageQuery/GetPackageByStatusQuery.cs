// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.PackageQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetPackageByStatusQuery : IRequest<MethodResult<List<PackageModel>>>
    {
        public EnumPackageStatus? Status { get; set; }
    }

    public class GetPackageByStatusQueryHandler : IRequestHandler<GetPackageByStatusQuery, MethodResult<List<PackageModel>>>
    {
        private readonly IPackageRepository _packageRepository;
        private readonly IMapper _mapper;

        public GetPackageByStatusQueryHandler(IPackageRepository packageRepository, IMapper mapper)
        {
            _packageRepository = packageRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<List<PackageModel>>> Handle(GetPackageByStatusQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<List<PackageModel>>();

            var packages = await _packageRepository.Queryable.ToListAsync(cancellationToken);

            if (request.Status.HasValue)
            {
                packages = packages.Where(p => p.Status == request.Status).ToList();
            }

            methodResult.Result = _mapper.Map<List<PackageModel>>(packages);
            return methodResult;
        }
    }
}
