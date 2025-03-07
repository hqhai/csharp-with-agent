// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.VoucherQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
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
        private readonly IEventRepository _eventRepository;

        public GetVoucherQueryHandler(IMapper mapper, IVoucherRepository voucherRepository, IEventRepository eventRepository)
        {
            _mapper = mapper;
            _voucherRepository = voucherRepository;
            _eventRepository = eventRepository;
        }

        public async Task<MethodResult<VoucherModel>> Handle(GetVoucherQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var methodResult = new MethodResult<VoucherModel>();

            var voucher = await _voucherRepository.Queryable.Include(p => p.VoucherPackages).Include(p => p.Orders).FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (voucher == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(voucher));
                return methodResult;
            }
            var voucherModel = _mapper.Map<VoucherModel>(voucher);

            voucherModel.QuantityUsed = voucher.Orders.Where(p => p.Status == EnumOrderStatus.New || p.Status == EnumOrderStatus.Payment).Count();

            var @events = await _eventRepository.Queryable.Where(p => voucherModel.EventIds != null && voucherModel.EventIds.Contains(p.Id)).ToListAsync(cancellationToken);

            voucherModel.EventModels = @events.Select(p => new EventModel()
            {
                Id = p.Id,
                Name = p.Name,
            }).ToList();

            methodResult.Result = voucherModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
