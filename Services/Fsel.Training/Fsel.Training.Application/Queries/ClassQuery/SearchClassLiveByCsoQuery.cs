// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.ClassQuery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Training.Application.Services.SystemServices;
    using Fsel.Training.Application.Services.UserServices;
    using Fsel.Training.Application.Services.UserServices.Models;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.EntityModels;
    using Fsel.Training.Domain.Models.QueryModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchClassLiveByCsoQuery : SearchClassLiveByCsoQueryModel, IRequest<MethodResult<PagingItemsModel<ClassLiveCalendarModel>>>
    {
    }

    public class SearchClassLiveByCsoQueryHandler : IRequestHandler<SearchClassLiveByCsoQuery, MethodResult<PagingItemsModel<ClassLiveCalendarModel>>>
    {
        private readonly IClassLiveCalendarRepository _classLiveCalendarRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly ISystemService _systemService;

        public SearchClassLiveByCsoQueryHandler(IClassLiveCalendarRepository classLiveCalendarRepository, AuthContext authContext, IUserService userService, ISystemService systemService)
        {
            _classLiveCalendarRepository = classLiveCalendarRepository;
            _authContext = authContext;
            _userService = userService;
            _systemService = systemService;
        }

        public async Task<MethodResult<PagingItemsModel<ClassLiveCalendarModel>>> Handle(SearchClassLiveByCsoQuery request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<PagingItemsModel<ClassLiveCalendarModel>>();
            ArgumentNullException.ThrowIfNull(request);
            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
            var csoResult = await _userService.GetCsoByUserIdAsync(_authContext.CurrentUserId);
            if (!csoResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError));
                return methodResult;
            }
            var csoId = csoResult.Content?.Result?.Id;
            var timeFramesResult = await _systemService.GetLiveTimeFramesAsync();
            var timeFrames = timeFramesResult.Content?.Result;
            var classLiveQuery = _classLiveCalendarRepository.Queryable
                                        .Include(x => x.Class)
                                        .Where(x => x.Class!.CsoId == csoId)
                                        .Select(x => new ClassLiveCalendarModel
                                        {
                                            Id = x.Id,
                                            ClassId = x.ClassId,
                                            ClassCode = x.Class!.Code,
                                            TeacherId = x.TeacherId,
                                            LiveTimeFrameId = x.LiveTimeFrameId,
                                            LiveDate = x.LiveDate,
                                            CreatedDate = x.CreatedDate
                                        });
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                classLiveQuery = classLiveQuery.Where(m => m.Id.ToString() == request.Keyword || (m.ClassCode ?? string.Empty).ToLower().Trim().Contains(request.Keyword.ToLower().Trim()));
            }

            int totalItem = await classLiveQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await classLiveQuery
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            var teacherResult = await _userService.GetTeacherByIdsAsync(new GetTeacherByIdsQueryModel { Ids = lists.Select(x => x.TeacherId ?? default).Distinct().ToList() });
            var teachers = teacherResult.Content?.Result;

            foreach (var item in lists)
            {
                var teacher = teachers!.FirstOrDefault(x => x.Id == item.TeacherId);
                item.TeacherName = teacher?.User?.FullName;
                var liveTimeFrame = timeFrames?.FirstOrDefault(x => x.Id == item.LiveTimeFrameId);
                item.StartTime = liveTimeFrame?.StartTime ?? default;
                item.EndTime = liveTimeFrame?.EndTime ?? default;
            }

            methodResult.Result = new PagingItemsModel<ClassLiveCalendarModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
