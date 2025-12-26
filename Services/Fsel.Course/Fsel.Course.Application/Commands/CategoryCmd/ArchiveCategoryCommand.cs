// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.CategoryCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class ArchiveCategoryCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }
    public class ArchiveCategoryCommandHandler : IRequestHandler<ArchiveCategoryCommand, MethodResult<bool>>
    {
        private readonly ICategoryRepository _categoryRepository;

        public ArchiveCategoryCommandHandler(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<MethodResult<bool>> Handle(ArchiveCategoryCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var category = await _categoryRepository.Queryable.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (category == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(category), request.Id);
                return methodResult;
            }

            await _categoryRepository.ExecuteTransactionAsync(async () =>
            {
                category.Status = EnumStatus.Archive;
                _categoryRepository.Update(category);
                await _categoryRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.Result = true;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            });

            return methodResult;
        }
    }
}
