// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.Cso
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Application.Services.UserServices.Models;
    using Fsel.Training.Application.Services.SystemServices;
    using Fsel.Training.Application.Services.UserServices;
    using Fsel.Training.Domain.Models.EntityModels;
    using Fsel.Training.Domain.Models.QueryModels;
    using Fsel.Training.Infrastructure.Repositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchTeacherFreeDateQuery : SearchTeacherFreeDateQueryModel, IRequest<MethodResult<PagingItemsModel<TeacherFreeDateModel>>>
    {
    }

    public class SearchTeacherFreeDateQueryHandler : IRequestHandler<SearchTeacherFreeDateQuery, MethodResult<PagingItemsModel<TeacherFreeDateModel>>>
    {
        private readonly TeacherFreeDateRepository _teacherFreeDateRepository;
        private readonly ISystemService _systemService;
        private readonly IUserService _userService;

        public SearchTeacherFreeDateQueryHandler(TeacherFreeDateRepository teacherFreeDateRepository, ISystemService systemService, IUserService userService)
        {
            _teacherFreeDateRepository = teacherFreeDateRepository;
            _systemService = systemService;
            _userService = userService;
        }

        public async Task<MethodResult<PagingItemsModel<TeacherFreeDateModel>>> Handle(SearchTeacherFreeDateQuery request, CancellationToken cancellationToken)
        {
            MethodResult<PagingItemsModel<TeacherFreeDateModel>> methodResult = new MethodResult<PagingItemsModel<TeacherFreeDateModel>>();
            ArgumentNullException.ThrowIfNull(request);
            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var teacherFreeDateQuery = _teacherFreeDateRepository.Queryable
                                        .Include(x => x.TeacherFreeTimes)
                                        .Select(x => new TeacherFreeDateModel
                                        {
                                            Id = x.Id,
                                            EndTime = x.EndTime,
                                            StartTime = x.StartTime,
                                            TeacherId = x.TeacherId,
                                            CreatedDate = x.CreatedDate,
                                            TeacherFreeTimes = x.TeacherFreeTimes.Select(x => new TeacherFreeTimeModel
                                            {
                                                Id = x.Id,
                                                DayOfWeek = x.DayOfWeek,
                                                LiveTimeFrameId = x.LiveTimeFrameId,
                                                Priority = x.Priority,
                                            }).ToList(),
                                        });

            int totalItem = await teacherFreeDateQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await teacherFreeDateQuery
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            var teacherResult = await _userService.GetTeacherByIdsAsync(new GetTeacherByIdsQueryModel { Ids = lists.Select(x => x.TeacherId).Distinct().ToList() });
            var teachers = teacherResult.Content?.Result;

            foreach (var item in lists)
            {
                var teacher = teachers!.FirstOrDefault(x => x.Id == item.TeacherId);
                item.TeacherName = teacher?.Human?.FullName;
            }
            var timeFrameResult = await _systemService.GetTimeFramByIdsAsync(teacherFreeDateQuery.SelectMany(x => x.TeacherFreeTimes!).Select(x => x.LiveTimeFrameId).ToList());
            var timeFrames = timeFrameResult.Content?.Result;

            foreach (var item in lists)
            {
                item.TimeFrameEndTime = timeFrames!.EndTime;
                item.TimeFrameStartTime = timeFrames!.StartTime;
            }
            methodResult.Result = new PagingItemsModel<TeacherFreeDateModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
