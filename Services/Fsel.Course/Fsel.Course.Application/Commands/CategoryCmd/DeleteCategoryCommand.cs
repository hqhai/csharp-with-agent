// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.CategoryCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class DeleteCategoryCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }

    public class DeleteCategoryCommandHanlder : IRequestHandler<DeleteCategoryCommand, MethodResult<bool>>
    {
        private readonly ICategoryRepository _categoryRepository;

        public DeleteCategoryCommandHanlder(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<MethodResult<bool>> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var category = await _categoryRepository.Queryable.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (category == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(category), request.Id);
                return methodResult;
            }

            List<Category> deleteCategories = new List<Category> { category };
            List<Category> parentCategories = new List<Category> { category };
            await AddChildentCategory(parentCategories, deleteCategories, cancellationToken);

            await _categoryRepository.ExecuteTransactionAsync(async () =>
            {
                await _categoryRepository.DeleteListAsync(deleteCategories);
                await _categoryRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.Result = true;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            });

            return methodResult;
        }

        private async Task AddChildentCategory(IList<Category> parentCategories, List<Category> categories, CancellationToken cancellationToken)
        {
            var parentCategoryIds = parentCategories.Select(x => x.Id).ToList();
            var childentCategories = await _categoryRepository.Queryable
                                                              .WhereBulkContains(parentCategoryIds, x => x.ParentId)
                                                              .ToListAsync(cancellationToken);

            if (childentCategories == null || !childentCategories.Any())
            {
                return;
            }

            categories.AddRange(childentCategories);

            await AddChildentCategory(childentCategories, categories, cancellationToken);
        }
    }
}
