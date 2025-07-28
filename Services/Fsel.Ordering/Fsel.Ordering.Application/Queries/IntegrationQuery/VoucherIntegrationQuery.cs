using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Fsel.Common.ActionResults;
using Fsel.Common.Helpers;
using Fsel.Ordering.Domain.Entities;
using Fsel.Ordering.Domain.IRepositories;
using Fsel.Ordering.Domain.Models.EntityModels.IntegrationModel;
using Fsel.Ordering.Infrastructure;
using Fsel.Ordering.Infrastructure.Repositories;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Ordering.Application.Queries.IntegrationQuery
{
    public class VoucherIntegrationQuery : IRequest<MethodResult<IList<VoucherIntegrationModel>>>
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? SourceName { get; set; }
    }

    public class VoucherIntegrationQueryHandler : IRequestHandler<VoucherIntegrationQuery, MethodResult<IList<VoucherIntegrationModel>>>
    {
        private readonly IVoucherRepository _voucherRepository;
        private readonly IMapper _mapper;

        public VoucherIntegrationQueryHandler(IVoucherRepository voucherRepository, IMapper mapper)
        {
            _voucherRepository = voucherRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<VoucherIntegrationModel>>> Handle(VoucherIntegrationQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<VoucherIntegrationModel>>();

            var query = from voucher in _voucherRepository.Queryable
                        select new
                        {
                            Voucher = voucher,
                            UserId = voucher.Orders.Select(x => x.UserId == Guid.Empty ? default(Guid?) : x.UserId).FirstOrDefault(),
                            Packages = voucher.VoucherPackages.Select(x => x.Package),
                        };

            //query = query.Where(x => x.Voucher.Source == Shared.Enums.EnumVoucherSource.MasterAgency);
            query = query.Where(x => !string.IsNullOrEmpty(x.Voucher.SourceName));

            if (request.StartDate.HasValue)
            {
                query = query.Where(x => x.Voucher.CreatedDate >= request.StartDate.Value);
            }
            if (request.EndDate.HasValue)
            {
                query = query.Where(x => x.Voucher.CreatedDate <= request.EndDate.Value);
            }
            if (!string.IsNullOrEmpty(request.SourceName))
            {
                query = query.Where(x => x.Voucher.SourceName == request.SourceName);
            }

            var dateNow = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);
            var data = await query.AsNoTracking().ToListAsync(cancellationToken);
            var result = data.Select(x =>
            {
                var voucher = _mapper.Map<VoucherIntegrationModel>(x.Voucher);
                voucher.Packages = _mapper.Map<IList<PackageIntegrationModel>>(x.Packages);
                voucher.UserId = x.UserId;
                if (x.UserId != null)
                {
                    voucher.Status = EnumVoucherIntegrationStatus.Redeemed;
                }
                else if (voucher.StartDate > dateNow)
                {
                    voucher.Status = EnumVoucherIntegrationStatus.Pending;
                }
                else if (voucher.EndDate < dateNow)
                {
                    voucher.Status = EnumVoucherIntegrationStatus.Expired;
                }
                else
                {
                    voucher.Status = EnumVoucherIntegrationStatus.Active;
                }
                return voucher;
            }).ToList();

            methodResult.Result = result;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
} 
