// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.ClassLiveWorkFlowQuery
{
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

    public class SearchChangeTeacherLivesByCsoQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<ChangeTeacherLiveModel>>>
    {
    }

    public class SearchChangeTeacherLiveByCsoQueryHandler : IRequestHandler<SearchChangeTeacherLivesByCsoQuery, MethodResult<PagingItemsModel<ChangeTeacherLiveModel>>>
    {
        private readonly IClassLiveWorkFlowRepository _classLiveWorkFlowRepository;
        private readonly ISystemService _systemService;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;

        public SearchChangeTeacherLiveByCsoQueryHandler(IClassLiveWorkFlowRepository classLiveWorkFlowRepository
            , ISystemService systemService
            , AuthContext authContext
            , IUserService userService)
        {
            _classLiveWorkFlowRepository = classLiveWorkFlowRepository;
            _systemService = systemService;
            _authContext = authContext;
            _userService = userService;
        }

        public async Task<MethodResult<PagingItemsModel<ChangeTeacherLiveModel>>> Handle(SearchChangeTeacherLivesByCsoQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<ChangeTeacherLiveModel>>();
            IList<Guid>? teacherIds = new List<Guid>();
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                var teachersResult = await _userService.GetTeachersByKeyword(request.Keyword);
                if (!teachersResult.IsSuccessStatusCode)
                {
                    methodResult.AddError(teachersResult.Error);
                    return methodResult;
                }
                if (teachersResult.Content?.Result == null)
                {
                    methodResult.Result = null;
                    return methodResult;
                }
                teacherIds = teachersResult.Content.Result.Select(x => x.Id).Distinct().ToList();
            }

            var csoResult = await _userService.GetCsoByUserIdAsync(_authContext.CurrentUserId);
            if (!csoResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError));
                return methodResult;
            }
            var csoId = csoResult.Content?.Result?.Id;

            var classLiveWorkFlows = _classLiveWorkFlowRepository.Queryable.Include(cld => cld.ClassLiveCalendar)
                .ThenInclude(c => c!.Class)
                .Where(p => p.Type == EnumWorkFlowType.ChangeTeacher && p.CsoId == csoId)
                .Select(ac => new ChangeTeacherLiveModel
                {
                    Id = ac.Id,
                    ClassName = ac.ClassLiveCalendar!.Class!.Code,
                    Type = ac.Type,
                    TeacherId = ac.TeacherId,
                    LiveTimeFrameId = ac.ClassLiveCalendar.LiveTimeFrameId,
                    Status = ac.Status,
                    LiveDate = ac.ClassLiveCalendar.LiveDate,
                    CreatedDate = ac.CreatedDate,
                    ClassLiveCalendarId = ac.ClassLiveCalendarId
                });
            if (teacherIds.Count >= 0 && !string.IsNullOrEmpty(request.Keyword))
            {
                classLiveWorkFlows = classLiveWorkFlows.Where(p => teacherIds.Contains(p.TeacherId ?? default));
            }
            int totalItem = await classLiveWorkFlows.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await classLiveWorkFlows
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            var liveTimeFramesResult = await _systemService.GetLiveTimeFramesAsync();
            if (!liveTimeFramesResult.IsSuccessStatusCode)
            {
                methodResult.AddError(liveTimeFramesResult.Error);
                return methodResult;
            }
            var liveTimeFrames = liveTimeFramesResult.Content?.Result;
            var teacherResult = await _userService.GetTeacherByIdsAsync(new GetTeacherByIdsQueryModel { Ids = lists.Select(x => x.TeacherId ?? default).Distinct().ToList() });
            var teachers = teacherResult.Content?.Result;

            foreach (var item in lists)
            {
                var teacher = teachers!.FirstOrDefault(x => x.Id == item.TeacherId);
                item.TeacherName = teacher?.User?.FullName;
                var liveTimeFrame = liveTimeFrames?.FirstOrDefault(p => p.Id == item.LiveTimeFrameId);
                item.StartTime = liveTimeFrame?.StartTime;
                item.EndTime = liveTimeFrame?.EndTime;
            }
            methodResult.Result = new PagingItemsModel<ChangeTeacherLiveModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
