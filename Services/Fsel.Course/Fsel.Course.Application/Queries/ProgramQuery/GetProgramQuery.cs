// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.ProgramQuery
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;

    public class GetProgramQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<ProgramModel>>>
    {
    }

    public class GetProgramQueryHandler : IRequestHandler<GetProgramQuery, MethodResult<PagingItemsModel<ProgramModel>>>
    {
        private readonly ICategoryRepository _categoryRepository;

        public GetProgramQueryHandler(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<MethodResult<PagingItemsModel<ProgramModel>>> Handle(GetProgramQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PagingItemsModel<ProgramModel>> methodResult = new MethodResult<PagingItemsModel<ProgramModel>>();

            var programQueries = _categoryRepository.Queryable.Where(x => x.Type == EnumTypeCategory.Program && x.Status == EnumStatus.Active).AsQueryable();

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                programQueries = programQueries.Where(x => !string.IsNullOrEmpty(x.Name) && x.Name.Contains(request.Keyword.Trim()));
            }

            return await _categoryRepository.GetListByPageResultAsync<ProgramModel>(programQueries, request, cancellationToken);
        }
    }
}
