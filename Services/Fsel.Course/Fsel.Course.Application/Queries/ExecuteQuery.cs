// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries
{
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;

    public class ExecuteQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<CourseModel>>>
    {
    }
    public class ExecuteQueryHandler : IRequestHandler<ExecuteQuery, MethodResult<PagingItemsModel<CourseModel>>>
    {
        private readonly ICourseRepository _courseRepository;
        public ExecuteQueryHandler(ICourseRepository courseRepository)
        {
            _courseRepository = courseRepository;
        }

        public async Task<MethodResult<PagingItemsModel<CourseModel>>> Handle(ExecuteQuery request, CancellationToken cancellationToken)
        {
            return await _courseRepository.GetListByPageAsync<CourseModel>(request, cancellationToken);
        }
    }
}
