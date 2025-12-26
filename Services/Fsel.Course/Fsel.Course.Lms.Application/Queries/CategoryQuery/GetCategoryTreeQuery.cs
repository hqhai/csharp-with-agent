// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.CategoryQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Services.ApplicationServices;

    public class GetCategoryTreeQuery : IRequest<MethodResult<CategoryTreeModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetCategoryTreeQueryHandler : IRequestHandler<GetCategoryTreeQuery, MethodResult<CategoryTreeModel>>
    {
        private readonly IMapper _mapper;
        private readonly ICategoryService _categoryService;


        public GetCategoryTreeQueryHandler(
            ICategoryService categoryService,
            IMapper mapper)
        {
            _categoryService = categoryService;
            _mapper = mapper;
        }

        public async Task<MethodResult<CategoryTreeModel>> Handle(GetCategoryTreeQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<CategoryTreeModel>();

            var categoryFound = await _categoryService.GetCategoryAsync(request.Id, cancellationToken);

            if (categoryFound == null)
            {
                return methodResult;
            }

            var categoryTreeModel = _mapper.Map<CategoryTreeModel>(categoryFound);
            categoryTreeModel.RemoveChildByCodition(x => x.ExpandedIcon != EnumTypeCategory.Program);
            methodResult.Result = categoryTreeModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
