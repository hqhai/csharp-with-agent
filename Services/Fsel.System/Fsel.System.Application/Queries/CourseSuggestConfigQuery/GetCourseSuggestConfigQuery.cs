// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.CourseSuggestConfigQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using Fsel.System.Domain.Models.QueryModels;
    using MediatR;

    public class GetCourseSuggestConfigQuery : SearchCourseSuggestConfigQueryModel, IRequest<MethodResult<PagingItemsModel<CourseSuggestConfigModel>>>
    {
    }

    public class GetCourseSuggestConfigQueryHandler : IRequestHandler<GetCourseSuggestConfigQuery, MethodResult<PagingItemsModel<CourseSuggestConfigModel>>>
    {
        private readonly ICourseSuggestConfigRepository _courseSuggestConfigRepository;

        public GetCourseSuggestConfigQueryHandler(ICourseSuggestConfigRepository courseSuggestConfigRepository)
        {
            _courseSuggestConfigRepository = courseSuggestConfigRepository;
        }

        public async Task<MethodResult<PagingItemsModel<CourseSuggestConfigModel>>> Handle(GetCourseSuggestConfigQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PagingItemsModel<CourseSuggestConfigModel>> methodResult = new MethodResult<PagingItemsModel<CourseSuggestConfigModel>>();

            var querys = _courseSuggestConfigRepository.Queryable.AsQueryable();

            if (request.FromAge != null)
            {
                querys = querys.Where(x => x.FromAge >= request.FromAge);
            }

            if (request.ToAge != null)
            {
                querys = querys.Where(x => x.ToAge <= request.ToAge);
            }

            if (request.PlacementTestLevel != null)
            {
                querys = querys.Where(x => x.PlacementTestLevel == request.PlacementTestLevel);
            }

            if (request.Type != null)
            {
                querys = querys.Where(x => x.Type == request.Type);
            }

            return await _courseSuggestConfigRepository.GetListByPageResultAsync<CourseSuggestConfigModel>(querys, request, cancellationToken);
        }
    }
}
