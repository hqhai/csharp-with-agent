// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.VoucherQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Ordering.Domain.Enums.ErrorCodes;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetVoucherQuery : IRequest<MethodResult<VoucherModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetVoucherQueryHandler : IRequestHandler<GetVoucherQuery, MethodResult<VoucherModel>>
    {
        private readonly IMapper _mapper;
        private readonly IVoucherRepository _voucherRepository;

        public GetVoucherQueryHandler(IMapper mapper, IVoucherRepository voucherRepository)
        {
            _mapper = mapper;
            _voucherRepository = voucherRepository;
        }

        public async Task<MethodResult<VoucherModel>> Handle(GetVoucherQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            MethodResult<VoucherModel> methodResult = new MethodResult<VoucherModel>();

            var voucherQuery = await _voucherRepository.Queryable
                                    .Include(x => x.VoucherPackages)
                                    .Select(x => new VoucherModel
                                    {
                                        Id = x.Id,
                                        Name = x.Name,
                                        StartDate = x.StartDate,
                                        EndDate = x.EndDate,
                                        CustomerType = x.CustomerType,
                                        CreatedDate = x.CreatedDate,
                                        IsActive = (x.IsActive == null ? (x.StartDate <= DateTime.Now && DateTime.Now <= x.EndDate) : x.IsActive),
                                        ContentFilePath = x.ContentFilePath,
                                        CourseLevels = x.CourseLevels,
                                        CreatedFullName = x.CreatedFullName,
                                        VoucherPackages = x.VoucherPackages.Select(x => new VoucherPackageModel
                                        {
                                            Id = x.Id,
                                            PackageId = x.PackageId,
                                            Percentage = x.Percentage,
                                            VoucherId = x.VoucherId,
                                        }).ToList(),
                                    }).FirstOrDefaultAsync(cancellationToken);

            if (voucherQuery == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumVoucherErrorCode.VoucherNotExist), nameof(request.Id), request.Id);
                return methodResult;
            }
            var voucherModel = _mapper.Map<VoucherModel>(voucherQuery);
            methodResult.Result = voucherModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
