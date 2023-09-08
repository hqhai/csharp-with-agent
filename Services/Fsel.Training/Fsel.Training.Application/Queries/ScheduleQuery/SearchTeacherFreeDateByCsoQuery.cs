// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.ScheduleQuery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Training.Application.Services.SystemServices;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.EntityModels;
    using Fsel.Training.Domain.Models.QueryModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchTeacherFreeDateByCsoQuery : SearchTeacherFreeDatesByCsoQueryModel, IRequest<MethodResult<IList<TeacherFreeTimeModel>>>
    {
    }

    public class SearchTeacherFreeDateByCsoQueryHandler : IRequestHandler<SearchTeacherFreeDateByCsoQuery, MethodResult<IList<TeacherFreeTimeModel>>>
    {
        private readonly ITeacherFreeTimeRepository _teacherFreeTimeRepository;
        private readonly ISystemService _systemService;

        public SearchTeacherFreeDateByCsoQueryHandler(ISystemService systemService, ITeacherFreeTimeRepository teacherFreeTimeRepository)
        {
            _systemService = systemService;
            _teacherFreeTimeRepository = teacherFreeTimeRepository;
        }

        public async Task<MethodResult<IList<TeacherFreeTimeModel>>> Handle(SearchTeacherFreeDateByCsoQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            MethodResult<IList<TeacherFreeTimeModel>> methodResult = new MethodResult<IList<TeacherFreeTimeModel>>();

            var teacherFreeTimeQuery = _teacherFreeTimeRepository.Queryable
                                    .Include(x => x.TeacherFreeDate)
                                    .Where(x => request.TeacherId.HasValue && x.TeacherFreeDate!.TeacherId == request.TeacherId)
                                    .Where(x => request.StartDate.HasValue && x.TeacherFreeDate!.StartDate.Date <= request.StartDate.Value.Date)
                                    .Where(x => request.EndDate.HasValue && x.TeacherFreeDate!.EndDate.Date >= request.EndDate.Value.Date)
                                    .Select(x => new TeacherFreeTimeModel
                                    {
                                        Id = x.Id,
                                        CreatedDate = x.CreatedDate,
                                        LiveTimeFrameId = x.LiveTimeFrameId,
                                        DayOfWeek = x.DayOfWeek
                                    });

            var lists = await teacherFreeTimeQuery
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            var timeFramesResult = await _systemService.GetLiveTimeFramesAsync();
            var timeFrames = timeFramesResult.Content?.Result;

            foreach (var item in lists)
            {
                var timeFrame = timeFrames?.FirstOrDefault(x => x.Id == item.LiveTimeFrameId);
                item.StartTime = timeFrame?.StartTime;
                item.EndTime = timeFrame?.EndTime;
            }

            methodResult.Result = lists;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
