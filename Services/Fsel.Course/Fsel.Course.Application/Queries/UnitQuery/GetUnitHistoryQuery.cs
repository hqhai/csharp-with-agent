// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.UnitQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetUnitHistoryQuery : IRequest<MethodResult<PagingItemsModel<UnitModel>>>
    {
        public Guid OriginalId { get; set; }

        public uint PageSize { get; set; } = 10;

        public uint PageIndex { get; set; } = 1;
    }

    public class GetUnitHistoryQueryHandler : IRequestHandler<GetUnitHistoryQuery, MethodResult<PagingItemsModel<UnitModel>>>
    {
        private readonly IUnitRepository _unitRepository;

        public GetUnitHistoryQueryHandler(IUnitRepository unitRepository)
        {
            _unitRepository = unitRepository;
        }

        public async Task<MethodResult<PagingItemsModel<UnitModel>>> Handle(GetUnitHistoryQuery request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return new MethodResult<PagingItemsModel<UnitModel>>
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            var totalCount = await _unitRepository.Queryable.CountAsync(u => u.OriginalId == request.OriginalId, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (totalCount < (request.PageIndex - 1) * request.PageSize)
            {
                return new MethodResult<PagingItemsModel<UnitModel>>
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            var units = await _unitRepository.Queryable
                .Where(u => u.OriginalId == request.OriginalId)
                .OrderByDescending(u => u.Version)
                .Skip((int)((request.PageIndex - 1) * request.PageSize))
                .Take((int)request.PageSize)
                .Select(u => new UnitModel
                {
                    Id = u.Id,
                    Name = u.Name,
                    Code = u.Code,
                    Version = u.Version,
                    CreatedDate = u.CreatedDate,
                    CreatedFullName = u.CreatedFullName,
                    CreatedUserId = u.CreatedUserId,
                    UpdatedDate = u.UpdatedDate,
                    UpdatedUserId = u.UpdatedUserId,
                    UpdatedFullName = u.UpdatedFullName,
                    OriginalId = u.OriginalId,
                })
                .AsNoTracking()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return new MethodResult<PagingItemsModel<UnitModel>>
            {
                Result = new PagingItemsModel<UnitModel>
                {
                    Items = units,
                    PagingInfo = new PagingInfoModel
                    {
                        TotalItems = totalCount,
                        PageSize = (int)request.PageSize,
                        Page = (int)request.PageIndex
                    }
                },
                StatusCode = StatusCodes.Status200OK
            };
        }
    }
}
