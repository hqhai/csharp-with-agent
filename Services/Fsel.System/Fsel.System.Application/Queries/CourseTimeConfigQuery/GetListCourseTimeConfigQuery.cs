// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.CourseTimeConfigQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetListCourseTimeConfigQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<CourseTimeConfigModel>>>
    {
    }

    public class GetListCourseTimeConfigQueryHandler : IRequestHandler<GetListCourseTimeConfigQuery, MethodResult<PagingItemsModel<CourseTimeConfigModel>>>
    {
        private readonly ICourseTimeConfigRepository _courseTimeConfigRepository;

        public GetListCourseTimeConfigQueryHandler(ICourseTimeConfigRepository courseTimeConfigRepository)
        {
            _courseTimeConfigRepository = courseTimeConfigRepository;
        }

        public async Task<MethodResult<PagingItemsModel<CourseTimeConfigModel>>> Handle(GetListCourseTimeConfigQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var methodResult = new MethodResult<PagingItemsModel<CourseTimeConfigModel>>();
            methodResult.Result = new PagingItemsModel<CourseTimeConfigModel>();

            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
            var courseTimeConfigQuery = _courseTimeConfigRepository.Queryable
                                    .Select(x => new CourseTimeConfigModel
                                    {
                                        Id = x.Id,
                                        CourseId = x.CourseId,
                                        CreatedDate = x.CreatedDate,
                                        DurationMonth = x.DurationMonth,
                                        EnrollmentWeek = x.EnrollmentWeek,
                                    });
            int totalItem = await courseTimeConfigQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await courseTimeConfigQuery
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            

            methodResult.Result = new PagingItemsModel<CourseTimeConfigModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
