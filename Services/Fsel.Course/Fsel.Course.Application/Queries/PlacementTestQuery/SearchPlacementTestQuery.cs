using Fsel.Common.ActionResults;
using Fsel.Core.Base.BaseModels;
using Fsel.Course.Common.Models.Entities;
using Fsel.Course.Common.Models.Queries;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Infrastructure.Repositories;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Course.Application.Querys.PlacementTestQuery
{
    public class SearchPlacementTestQuery : SearchPlacementTestQueryModel, IRequest<MethodResult<PagingItemsModel<PlacementTestModel>>>
    {
    }
    public class SearchPlacementTestQueryHandler : IRequestHandler<SearchPlacementTestQuery, MethodResult<PagingItemsModel<PlacementTestModel>>>
    {
        private readonly IPlacementTestRepository _placementTestRepository;
        private readonly IMapper _mapper;
        public SearchPlacementTestQueryHandler(IMapper mapper, IPlacementTestRepository placementTestRepository)
        {
            _placementTestRepository = placementTestRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<PagingItemsModel<PlacementTestModel>>> Handle(SearchPlacementTestQuery request, CancellationToken cancellationToken)
        {
            MethodResult<PagingItemsModel<PlacementTestModel>> methodResult = new MethodResult<PagingItemsModel<PlacementTestModel>>();

            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var placementTestQuery = from i in _placementTestRepository.Queryable
                                    select new PlacementTestModel
                                    {
                                        Id = i.Id,
                                        Name = i.Name,
                                        InstructionContent = i.InstructionContent,
                                        IsActive = i.IsActive,
                                        CourseLevel = i.CourseLevel,
                                        CreatedDate = i.CreatedDate,
                                        CreatedUserId = i.CreatedUserId,
                                        UpdatedDate = i.UpdatedDate,
                                        UpdatedUserId = i.UpdatedUserId,
                                    };
            //Keyword
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                placementTestQuery = placementTestQuery.Where(m => m.Id.ToString() == request.Keyword || (m.Name ?? string.Empty).Contains(request.Keyword));
            }

            int totalItem = await placementTestQuery.CountAsync().ConfigureAwait(false);
            var lists = await placementTestQuery.OrderByDescending(x => x.Id)
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .AsNoTracking()
                    .ToListAsync()
                    .ConfigureAwait(false);

            methodResult.Result = new PagingItemsModel<PlacementTestModel>
            {
                Items = _mapper.Map<IEnumerable<PlacementTestModel>>(lists),
                PagingInfo = new PagingInfoModel { Page = request.Page, PageSize = request.PageSize, TotalItems = totalItem }
            };

            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
