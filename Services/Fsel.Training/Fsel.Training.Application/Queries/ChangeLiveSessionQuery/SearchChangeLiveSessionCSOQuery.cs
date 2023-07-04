// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.ChangeLiveSessionQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Training.Application.Services.SystemServices;
    using Fsel.Training.Application.Services.UserServices;
    using Fsel.Training.Application.Services.UserServices.Models;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchChangeLiveSessionCSOQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<ChangeLiveSessionModel>>>
    {
    }

    public class SearchChangeLiveSessionCSOQueryHandler : IRequestHandler<SearchChangeLiveSessionCSOQuery, MethodResult<PagingItemsModel<ChangeLiveSessionModel>>>
    {
        private readonly IClassLiveWorkFlowRepository _classLiveWorkFlowRepository;
        private readonly IUserService _userService;
        private readonly ISystemService _systemService;

        public SearchChangeLiveSessionCSOQueryHandler(
            IClassLiveWorkFlowRepository classLiveWorkFlowRepository,
            IUserService userService,
            ISystemService systemService)
        {
            _classLiveWorkFlowRepository = classLiveWorkFlowRepository;
            _userService = userService;
            _systemService = systemService;
        }

        public async Task<MethodResult<PagingItemsModel<ChangeLiveSessionModel>>> Handle(SearchChangeLiveSessionCSOQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<ChangeLiveSessionModel>>();
            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
            var query = _classLiveWorkFlowRepository.Queryable
                        .Include(x => x.ClassLiveCalendar)
                        .ThenInclude(x => x!.Class)
                        .AsNoTracking()
                        .Select(x => new ChangeLiveSessionModel
                        {
                            Id = x.Id,
                            ClassName = x.ClassLiveCalendar!.Class!.Name,
                            CreatedDate = x.CreatedDate,
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

            var teacherResult = await _userService.GetTeacherByIdsAsync(new GetTeacherByIdsQueryModel { Ids = lists.Select(x => x.Id).Distinct().ToList() });
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

            methodResult.Result = new PagingItemsModel<ChangeLiveSessionModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
