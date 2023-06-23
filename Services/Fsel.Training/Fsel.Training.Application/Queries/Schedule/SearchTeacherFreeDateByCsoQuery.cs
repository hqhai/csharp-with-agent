// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.Schedule
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Application.Services.UserServices.Models;
    using Fsel.Training.Application.Services.SystemServices;
    using Fsel.Training.Application.Services.UserServices;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.EntityModels;
    using Fsel.Training.Domain.Models.QueryModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchTeacherFreeDateByCsoQuery : SearchTeacherFreeDatesByCsoQueryModel, IRequest<MethodResult<PagingItemsModel<TeacherFreeDateModel>>>
    {
    }

    public class GetListTeacherLiveTimeQueryHandler : IRequestHandler<SearchTeacherFreeDateByCsoQuery, MethodResult<PagingItemsModel<TeacherFreeDateModel>>>
    {
        private readonly ITeacherFreeDateRepository _teacherFreeDateRepository;
        private readonly ISystemService _systemService;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public GetListTeacherLiveTimeQueryHandler(ITeacherFreeDateRepository teacherFreeDateRepository, ISystemService systemService, IMapper mapper, IUserService userService)
        {
            _teacherFreeDateRepository = teacherFreeDateRepository;
            _systemService = systemService;
            _mapper = mapper;
            _userService = userService;
        }

        public async Task<MethodResult<PagingItemsModel<TeacherFreeDateModel>>> Handle(SearchTeacherFreeDateByCsoQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            MethodResult<PagingItemsModel<TeacherFreeDateModel>> methodResult = new MethodResult<PagingItemsModel<TeacherFreeDateModel>>();

            var teacherFreeDateQuery = _teacherFreeDateRepository.Queryable
                                    .Include(x => x.TeacherFreeTimes)
                                    .Select(x => new TeacherFreeDateModel
                                    {
                                        Id = x.Id,
                                        StartTime = x.StartTime,
                                        EndTime = x.EndTime,
                                        TeacherId = x.TeacherId,
                                        CreatedDate = x.CreatedDate,
                                        TeacherFreeTimes = x.TeacherFreeTimes.Select(x => new TeacherFreeTimeModel
                                        {
                                            Id = x.Id,
                                            LiveTimeFrameId = x.LiveTimeFrameId,
                                            DayOfWeek = x.DayOfWeek,
                                        }).ToList(),
                                    });
            if (request.TeacherId != null)
            {
                teacherFreeDateQuery = teacherFreeDateQuery.Where(m => m.TeacherId == request.TeacherId);
            }
            if (request.StartTime != null)
            {
                teacherFreeDateQuery = teacherFreeDateQuery.Where(m => m.StartTime == request.StartTime);
            }
            if (request.EndTime != null)
            {
                teacherFreeDateQuery = teacherFreeDateQuery.Where(m => m.EndTime == request.EndTime);
            }
            if (request.StartTime < request.EndTime)
            {
            }
            int totalItem = await teacherFreeDateQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await teacherFreeDateQuery
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            var teacherResult = await _userService.GetTeacherByIdsAsync(new GetTeacherByIdsQueryModel { Ids = teacherFreeDateQuery.Select(x => x.TeacherId).Distinct().ToList() });
            var teachers = teacherResult.Content?.Result;

            foreach (var item in teacherFreeDateQuery)
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
