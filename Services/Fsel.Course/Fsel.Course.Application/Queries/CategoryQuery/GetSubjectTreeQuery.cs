// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.CategoryQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetSubjectTreeQuery : IRequest<MethodResult<IList<CategoryTreeModel>>>
    {
    }

    public class GetSubjectTreeQueryHandler : IRequestHandler<GetSubjectTreeQuery, MethodResult<IList<CategoryTreeModel>>>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        private readonly ProgramConverter _programConverter;

        public GetSubjectTreeQueryHandler(ICategoryRepository categoryRepository,
            IMapper mapper,
            ProgramConverter programConverter)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
            _programConverter = programConverter;
        }

        public async Task<MethodResult<IList<CategoryTreeModel>>> Handle(GetSubjectTreeQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<CategoryTreeModel>>();
            var subjects = await _categoryRepository.Queryable.Where(x => !x.ParentId.HasValue && x.Status == EnumStatus.Active)
                                                    .ToListAsync(cancellationToken);

            var categoryTrees = _mapper.Map<IList<CategoryTreeModel>>(subjects);
            await _programConverter.AddChildentCategory(categoryTrees, cancellationToken);

            methodResult.Result = categoryTrees.Where(x => x.Children != null && x.Children.Any()).OrderByDescending(x => x.UpdatedDate ?? x.CreatedDate).ToList();
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
