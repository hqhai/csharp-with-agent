// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.CourseQuery
{
    using System;
    using System.Linq.Dynamic.Core;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetHistoryCourseQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<CourseHistoryModel>>>
    {
        public Guid OriginalId { get; set; }
    }

    public class GetHistoryCourseQueryHandler : IRequestHandler<GetHistoryCourseQuery, MethodResult<PagingItemsModel<CourseHistoryModel>>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;

        public GetHistoryCourseQueryHandler(ICourseRepository courseRepository,
                                            IMapper mapper)
        {
            _courseRepository = courseRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<PagingItemsModel<CourseHistoryModel>>> Handle(GetHistoryCourseQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PagingItemsModel<CourseHistoryModel>> methodResult = new MethodResult<PagingItemsModel<CourseHistoryModel>>();

            var courses = _courseRepository.Queryable
                                           .Where(x => x.OriginalId == request.OriginalId)
                                           .OrderByDescending(x => x.Version)
                                           .AsNoTracking();
            if (courses == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(courses));
                return methodResult;
            }

            int totalItem = await courses.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

            var lists = await courses.ApplySortAndPaging(request)
                                     .AsNoTracking()
                                     .ToListAsync(cancellationToken: cancellationToken)
                                     .ConfigureAwait(false);


            methodResult.Result = new PagingItemsModel<CourseHistoryModel>(_mapper.Map<IList<CourseHistoryModel>>(lists), request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
