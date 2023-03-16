using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Core.Base.BaseModels;

using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Course.Domain.Models.QueryModels.Units;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Application.Queries.UnitQuery
{
    public class SearchUnitQuery : SearchUnitQueryModel, IRequest<MethodResult<PagingItemsModel<UnitModel>>>
    {
    }

    public class SearchUnitQueryHandler : IRequestHandler<SearchUnitQuery, MethodResult<PagingItemsModel<UnitModel>>>
    {
        private readonly IUnitRepository _UnitRepository;
        private readonly IMapper _mapper;

        public SearchUnitQueryHandler(IMapper mapper, IUnitRepository UnitRepository)
        {
            _UnitRepository = UnitRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<PagingItemsModel<UnitModel>>> Handle(SearchUnitQuery request, CancellationToken cancellationToken)
        {
            MethodResult<PagingItemsModel<UnitModel>> methodResult = new MethodResult<PagingItemsModel<UnitModel>>();

            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var UnitQuery = from i in _UnitRepository.Queryable
                                    .Include(x => x.CourseUnitMockTests.Where(y => !y.IsDeleted))
                            select new UnitModel
                            {
                                Id = i.Id,
                                Name = i.Name,
                                DisplayName = i.DisplayName,
                                IsActive = !i.CourseUnitMockTests.Any(),
                                Type = i.Type,
                                CourseLevel = i.CourseLevel,
                                CreatedDate = i.CreatedDate,
                                CreatedUserId = i.CreatedUserId,
                                UpdatedDate = i.UpdatedDate,
                                UpdatedUserId = i.UpdatedUserId,
                            };
            //Keyword
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                UnitQuery = UnitQuery.Where(m => m.Id.ToString() == request.Keyword || (m.Name ?? string.Empty).Contains(request.Keyword));
            }

            int totalItem = await UnitQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await UnitQuery.OrderByDescending(x => x.Id)
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            methodResult.Result = new PagingItemsModel<UnitModel>
            {
                Items = _mapper.Map<IEnumerable<UnitModel>>(lists),
                PagingInfo = new PagingInfoModel { Page = request.Page, PageSize = request.PageSize, TotalItems = totalItem }
            };

            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
