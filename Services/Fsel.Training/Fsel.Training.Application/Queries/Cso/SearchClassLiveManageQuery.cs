// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.Cso
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Training.Application.Services.CourseServices;
    using Fsel.Training.Application.Services.SystemServices;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.EntityModels;
    using Fsel.Training.Domain.Models.QueryModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchClassLiveManageQuery : SearchClassLiveManageQueryModel, IRequest<MethodResult<PagingItemsModel<ClassModel>>>
    {
    }

    public class SearchClassLiveManageQueryHandler : IRequestHandler<SearchClassLiveManageQuery, MethodResult<PagingItemsModel<ClassModel>>>
    {
        private readonly IMapper _mapper;
        private readonly IClassRepository _classRepository;
        private readonly ICourseService _courseService;
        private readonly ISystemService _systemService;

        public SearchClassLiveManageQueryHandler(IMapper mapper, IClassRepository classRepository, ICourseService courseService, ISystemService systemService)
        {
            _mapper = mapper;
            _classRepository = classRepository;
            _courseService = courseService;
            _systemService = systemService;
        }

        public async Task<MethodResult<PagingItemsModel<ClassModel>>> Handle(SearchClassLiveManageQuery request, CancellationToken cancellationToken)
        {
            MethodResult<PagingItemsModel<ClassModel>> methodResult = new MethodResult<PagingItemsModel<ClassModel>>();
            ArgumentNullException.ThrowIfNull(request);
            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var classQuery = _classRepository.Queryable
                            .Select(x => new ClassModel
                            {
                                Id = x.Id,
                                Name = x.Name,
                                Code = x.Code,
                                CourseId = x.CourseId,
                                CreatedDate = x.CreatedDate,
                                LiveTimeFrameId = x.LiveTimeFrameId,
                            });

            if (request.Code != null)
            {
                classQuery = classQuery.Where(m => m.Code == request.Code);
            }
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                classQuery = classQuery.Where(m => m.Id.ToString() == request.Keyword || (m.Name ?? string.Empty).Contains(request.Keyword));
            }

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                classQuery = classQuery.Where(m => m.Id.ToString() == request.Keyword || (m.Name ?? string.Empty).Contains(request.Keyword));
            }

            int totalItem = await classQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await classQuery
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            foreach (var item in lists)
            {
                var courseLevel = await _courseService.GetCourseByIdAsync(item.CourseId);
                var result = courseLevel.Content?.Result;
                if (result != null)
                {
                    item.CourseLevel = result.CourseLevel;
                }
            }
            var timeFrameResult = await _systemService.GetTimeFramByIdsAsync(classQuery.Select(x => x.LiveTimeFrameId ?? default).ToList());
            var timeFrames = timeFrameResult.Content?.Result;

            foreach (var item in lists)
            {
                item.TimeFrameEndTime = timeFrames!.EndTime;
                item.TimeFrameStartTime = timeFrames!.StartTime;
            }

            methodResult.Result = new PagingItemsModel<ClassModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
