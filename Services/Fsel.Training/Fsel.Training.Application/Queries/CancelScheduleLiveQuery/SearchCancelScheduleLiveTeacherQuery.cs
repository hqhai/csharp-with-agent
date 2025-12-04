// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.CancelScheduleLiveQuery
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
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
        private readonly AuthContext _authContext;

        public SearchCancelScheduleLiveTeacherQueryHandler(IClassLiveWorkFlowRepository classLiveWorkFlowRepository, IUserService userService, ISystemService systemService, AuthContext authContext)
        {
            _classLiveWorkFlowRepository = classLiveWorkFlowRepository;
            _userService = userService;
            _systemService = systemService;
            _authContext = authContext;
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
            var teacherIdResult = await _userService.GetTeacherByUserIdAsync(_authContext.CurrentUserId);
            if (!teacherIdResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError));
                return methodResult;
            }
            var teacherId = teacherIdResult.Content?.Result?.Id;
            var query = _classLiveWorkFlowRepository.Queryable
                        .Include(x => x.ClassLiveCalendar)
                        .ThenInclude(x => x!.Class)
                        .Where(x => x.Type == EnumWorkFlowType.CancelSchedule && x.TeacherId == teacherId)
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
                query = query.Where(p => (p.ClassName ?? string.Empty).ToLower().Trim().Contains(request.Keyword.ToLower().Trim()));
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
                item.TeacherName = teachers?.FirstOrDefault(x => x.Id == item.TeacherId)?.User?.FullName;
            }

            methodResult.Result = new PagingItemsModel<CancelScheduleLiveModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
