// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.CancelScheduleLiveQuery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Shared.Enums;
    using Fsel.Training.Application.Services.SystemServices;
    using Fsel.Training.Application.Services.UserServices;
    using Fsel.Training.Application.Services.UserServices.Models;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchCancelScheduleLiveTeacherQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<CancelScheduleLiveModel>>>
    {
    }

    public class SearchCancelScheduleLiveTeacherQueryHandler : IRequestHandler<SearchCancelScheduleLiveTeacherQuery, MethodResult<PagingItemsModel<CancelScheduleLiveModel>>>
    {
        private readonly IClassLiveWorkFlowRepository _classLiveWorkFlowRepository;
        private readonly IUserService _userService;
        private readonly ISystemService _systemService;

        public SearchCancelScheduleLiveTeacherQueryHandler(IClassLiveWorkFlowRepository classLiveWorkFlowRepository, IUserService userService, ISystemService systemService)
        {
            _classLiveWorkFlowRepository = classLiveWorkFlowRepository;
            _userService = userService;
            _systemService = systemService;
        }

        public async Task<MethodResult<PagingItemsModel<CancelScheduleLiveModel>>> Handle(SearchCancelScheduleLiveTeacherQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<CancelScheduleLiveModel>>();
            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
            var query = _classLiveWorkFlowRepository.Queryable
                        .Include(x => x.ClassLiveCalendar)
                        .ThenInclude(x => x!.Class)
                        .Where(x => x.Type == EnumWorkFlowType.CancelSchedule)
                        .AsNoTracking()
                        .Select(x => new CancelScheduleLiveModel
                        {
                            Id = x.Id,
                            ClassName = x.ClassLiveCalendar!.Class!.Name,
                            CreatedDate = x.CreatedDate,
                            TeacherId = x.TeacherId ?? default,
                            LiveTimeFrameId = x.ClassLiveCalendar.LiveTimeFrameId,
                            Status = x.Status
                        });

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                query = query.Where(p => !string.IsNullOrEmpty(p.ClassName) && p.ClassName.Contains(request.Keyword));
            }

            int totalItem = await query.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await query
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            var teacherResult = await _userService.GetTeacherByIdsAsync(new GetTeacherByIdsQueryModel { Ids = lists.Select(x => x.TeacherId).Distinct().ToList() });
            var teachers = teacherResult.Content?.Result;
            var timeFramesResult = await _systemService.GetLiveTimeFramesAsync();
            var timeFrames = timeFramesResult.Content?.Result;
            foreach (var item in lists)
            {
                var liveTimeFrame = timeFrames?.FirstOrDefault(x => x.Id == item.LiveTimeFrameId);
                item.StartTime = liveTimeFrame?.StartTime;
                item.EndTime = liveTimeFrame?.EndTime;
                item.TeacherName = teachers?.FirstOrDefault(x => x.Id == item.TeacherId)?.Human?.FullName;
            }

            methodResult.Result = new PagingItemsModel<CancelScheduleLiveModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
