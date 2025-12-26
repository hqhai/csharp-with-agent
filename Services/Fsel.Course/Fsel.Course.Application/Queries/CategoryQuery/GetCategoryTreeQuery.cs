// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.CategoryQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetCategoryTreeQuery : IRequest<MethodResult<IList<CategoryTreeModel>>>
    {
        public Guid Id { get; set; }
    }

    public class GetCategoryTreeQueryHandler : IRequestHandler<GetCategoryTreeQuery, MethodResult<IList<CategoryTreeModel>>>
    {
        private readonly ProgramConverter _programConverter;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public GetCategoryTreeQueryHandler(ProgramConverter programConverter,
                                           ICategoryRepository categoryRepository,
                                           IMapper mapper)
        {
            _programConverter = programConverter;
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<CategoryTreeModel>>> Handle(GetCategoryTreeQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<CategoryTreeModel>> methodResult = new MethodResult<IList<CategoryTreeModel>>();

            var categories = await _categoryRepository.Queryable.Where(x => x.ParentId == request.Id && x.Status != EnumStatus.Archive).ToListAsync(cancellationToken);
            if (categories == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(categories));
                return methodResult;
            }

            var categoryTrees = _mapper.Map<IList<CategoryTreeModel>>(categories);
            await _programConverter.AddChildentCategory(categoryTrees, cancellationToken);

            methodResult.Result = categoryTrees.OrderByDescending(x => x.CreatedDate).ToList();
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
