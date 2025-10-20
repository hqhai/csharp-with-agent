// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.HomeWorkQuery
{
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.HomeWorks;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchHomeWorkQuery : SearchHomeWorkQueryModel, IRequest<MethodResult<PagingItemsModel<HomeWorkSearchModel>>>
    {
    }

    public class SearchHomeWorkQueryHandler : IRequestHandler<SearchHomeWorkQuery, MethodResult<PagingItemsModel<HomeWorkSearchModel>>>
    {
        private readonly IHomeWorkRepository _homeWorkRepository;

        public SearchHomeWorkQueryHandler(IHomeWorkRepository homeWorkRepository)
        {
            _homeWorkRepository = homeWorkRepository;
        }

        public async Task<MethodResult<PagingItemsModel<HomeWorkSearchModel>>> Handle(SearchHomeWorkQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PagingItemsModel<HomeWorkSearchModel>> methodResult = new MethodResult<PagingItemsModel<HomeWorkSearchModel>>();

            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var homeWorkQuery = _homeWorkRepository.Queryable.Where(p => !p.IsArchive && p.Type == request.Type)
                                    .Include(x => x.LessonHomeWorks.Where(n => !n.IsDeleted))
                                    .Select(x => new HomeWorkSearchModel
                                    {
                                        Id = x.Id,
                                        Code = x.Code,
                                        Name = x.Name,
                                        Type = x.Type,
                                        CreatedFullName = x.CreatedFullName,
                                        CreatedDate = x.CreatedDate,
                                        IsActive = x.LessonHomeWorks.Where(n => !n.IsDeleted).Any(),
                                        CourseLevel = x.CourseLevel,
                                        CourseSkill = x.CourseSkill,
                                    });

            request.Keyword = request.Keyword?.Trim().ToLower(CultureInfo.CurrentCulture);
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                if (Guid.TryParse(request.Keyword, out var guid))
                {
                    homeWorkQuery = homeWorkQuery.Where(m => m.Id == guid);
                }
                else
                {
                    homeWorkQuery = homeWorkQuery.Where(m => m.Code != null && m.Code.Contains(request.Keyword));
                }
            }

            if (request.CourseLevel != null)
            {
                homeWorkQuery = homeWorkQuery.Where(m => m.CourseLevel == request.CourseLevel);
            }

            if (request.CourseSkill != null)
            {
                homeWorkQuery = homeWorkQuery.Where(m => m.CourseSkill == request.CourseSkill);
            }

            request.SortBy.Add(new Common.Models.GenericSortModel
            {
                Property = nameof(HomeWorkSearchModel.Code),
                IsDesc = false
            });

            int totalItem = await homeWorkQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await homeWorkQuery
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            methodResult.Result = new PagingItemsModel<HomeWorkSearchModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
