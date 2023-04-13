// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.HomeWorkQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.HomeWorks;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchHomeWorkQuery : SearchHomeWorkQueryModel, IRequest<MethodResult<PagingItemsModel<HomeWorkSearchModel>>>
    {
    }

    public class SearchLessonQueryHandler : IRequestHandler<SearchHomeWorkQuery, MethodResult<PagingItemsModel<HomeWorkSearchModel>>>
    {
        private readonly IMapper _mapper;
        private readonly IHomeWorkRepository _homeWorkRepository;

        public SearchLessonQueryHandler(IMapper mapper, IHomeWorkRepository homeWorkRepository)
        {
            _mapper = mapper;
            _homeWorkRepository = homeWorkRepository;
        }

        public async Task<MethodResult<PagingItemsModel<HomeWorkSearchModel>>> Handle(SearchHomeWorkQuery request, CancellationToken cancellationToken)
        {
            MethodResult<PagingItemsModel<HomeWorkSearchModel>> methodResult = new MethodResult<PagingItemsModel<HomeWorkSearchModel>>();
            ArgumentNullException.ThrowIfNull(request);
            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
            var homeWorkQuery = _homeWorkRepository.Queryable
                        .Include(x => x.LessonHomeWorks.Where(n => !n.IsDeleted))
                        .Select(x => new HomeWorkSearchModel
                        {
                            Id = x.Id,
                            Name = x.Name,
                            CreatedFullName = x.CreatedFullName,
                            CreatedDate = x.CreatedDate,
                            IsActive = x.LessonHomeWorks.Where(n => !n.IsDeleted).Any(),
                            CourseLevel = x.CourseLevel,
                            CourseSkill = x.CourseSkill,
                        });
            //Keyword
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                homeWorkQuery = homeWorkQuery.Where(m => m.Id.ToString() == request.Keyword || (m.Name ?? string.Empty).Contains(request.Keyword));
            }

            if (request.CourseLevel != null)
            {
                homeWorkQuery = homeWorkQuery.Where(m => m.CourseLevel == request.CourseLevel);
            }

            if (request.CourseSkill != null)
            {
                homeWorkQuery = homeWorkQuery.Where(m => m.CourseSkill == request.CourseSkill);
            }

            int totalItem = await homeWorkQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await homeWorkQuery.OrderByDescending(x => x.CreatedDate)
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            methodResult.Result = new PagingItemsModel<HomeWorkSearchModel>
            {
                Items = _mapper.Map<IEnumerable<HomeWorkSearchModel>>(lists),
                PagingInfo = new PagingInfoModel { Page = request.Page, PageSize = request.PageSize, TotalItems = totalItem }
            };

            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
