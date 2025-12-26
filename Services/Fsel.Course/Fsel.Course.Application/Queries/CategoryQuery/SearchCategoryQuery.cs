// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.CategoryQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;

    public class SearchCategoryQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<CategoryModel>>>
    {
    }

    public class SearchCategoryQueryHandler : IRequestHandler<SearchCategoryQuery, MethodResult<PagingItemsModel<CategoryModel>>>
    {
        private readonly ICategoryRepository _categoryRepository;

        public SearchCategoryQueryHandler(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<MethodResult<PagingItemsModel<CategoryModel>>> Handle(SearchCategoryQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PagingItemsModel<CategoryModel>> methodResult = new MethodResult<PagingItemsModel<CategoryModel>>();

            var categoryQueries = _categoryRepository.Queryable.Where(x => x.Type == EnumTypeCategory.Subject && !x.ParentId.HasValue && x.Status != EnumStatus.Archive).AsQueryable();

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                categoryQueries = categoryQueries.Where(x => !string.IsNullOrEmpty(x.Name) && x.Name.Contains(request.Keyword.Trim()));
            }

            return await _categoryRepository.GetListByPageResultAsync<CategoryModel>(categoryQueries, request, cancellationToken);
        }
    }
}
