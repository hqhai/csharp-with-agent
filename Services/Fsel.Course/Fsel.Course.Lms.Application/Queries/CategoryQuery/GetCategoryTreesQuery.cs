// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.CategoryQuery
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetCategoryTreesQuery : IRequest<MethodResult<IList<CategoryTreeDtoModel>>>
    {
    }

    public class GetCategoryTreesQueryHandler : IRequestHandler<GetCategoryTreesQuery, MethodResult<IList<CategoryTreeDtoModel>>>
    {
        private readonly ICategoryRepository _categoryRepository;

        public GetCategoryTreesQueryHandler(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<MethodResult<IList<CategoryTreeDtoModel>>> Handle(GetCategoryTreesQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<CategoryTreeDtoModel>>();

            var categories = await _categoryRepository.Queryable.Include(p => p.Levels).AsNoTracking().ToListAsync(cancellationToken);

            List<CategoryTreeDtoModel> BuildCategoryTree(List<Category> categories, Guid? parentId, bool isRoot = false)
            {
                return categories
                    .Where(x =>
                        x.ParentId == parentId &&
                        (
                            x.Type == EnumTypeCategory.Subject ||
                            (!isRoot && x.Type == EnumTypeCategory.Program)
                        ))
                    .Select(x => new CategoryTreeDtoModel
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Type = x.Type,

                        Levels = x.Type == EnumTypeCategory.Program
                            ? x.Levels.OrderBy(l => l.LevelOrder).Select(l => new LevelTreeDtoModel
                            {
                                Id = l.Id,
                                Name = l.Name
                            }).ToList()
                            : new List<LevelTreeDtoModel>(),

                        Children = x.Type == EnumTypeCategory.Subject
                            ? BuildCategoryTree(categories, x.Id)
                            : new List<CategoryTreeDtoModel>()
                    })
                    .ToList();
            }

            var tree = BuildCategoryTree(categories, null, true);

            methodResult.Result = tree;
            return methodResult;
        }
    }
}
