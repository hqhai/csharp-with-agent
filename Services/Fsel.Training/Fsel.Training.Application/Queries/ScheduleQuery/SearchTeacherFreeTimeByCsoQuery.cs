// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.ScheduleQuery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Training.Application.Services.UserServices;
    using Fsel.Training.Application.Services.UserServices.Models;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.EntityModels;
    using Fsel.Training.Domain.Models.QueryModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchTeacherFreeTimeByCsoQuery : SearchTeacherFreeTimeByCsoQueryModel, IRequest<MethodResult<PagingItemsModel<TeacherFreeTimeModel>>>
    {
    }

    public class SearchTeacherFreeTimeByCsoQueryHandler : IRequestHandler<SearchTeacherFreeTimeByCsoQuery, MethodResult<PagingItemsModel<TeacherFreeTimeModel>>>
    {
        private readonly ITeacherFreeTimeRepository _teacherFreeTimeRepository;
        private readonly IUserService _userService;

        public SearchTeacherFreeTimeByCsoQueryHandler(ITeacherFreeTimeRepository teacherFreeTimeRepository, IUserService userService)
        {
            _teacherFreeTimeRepository = teacherFreeTimeRepository;
            _userService = userService;
        }

        public async Task<MethodResult<PagingItemsModel<TeacherFreeTimeModel>>> Handle(SearchTeacherFreeTimeByCsoQuery request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<PagingItemsModel<TeacherFreeTimeModel>>();
            ArgumentNullException.ThrowIfNull(request);
            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var teacherFreeTimeQuery = _teacherFreeTimeRepository.Queryable
                                    .Include(x => x.TeacherFreeDate)
                                    .Where(x => request.StartDate == null || x.TeacherFreeDate!.StartDate <= request.StartDate.Value.Date)
                                    .Where(x => request.EndDate == null || x.TeacherFreeDate!.EndDate >= request.EndDate.Value.Date)
                                    .Where(x => request.DayOfWeek == null || x.DayOfWeek == request.DayOfWeek)
                                    .Where(x => request.Priority == null || x.Priority == request.Priority)
                                    .Where(x => request.LiveTimeFrameId == null || x.LiveTimeFrameId == request.LiveTimeFrameId)
                                    .Select(x => new TeacherFreeTimeModel
                                    {
                                        Id = x.Id,
                                        CreatedDate = x.CreatedDate,
                                        Priority = x.Priority,
                                        TeacherId = x.TeacherFreeDate!.TeacherId,
                                    });

            int totalItem = await teacherFreeTimeQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await GetTeacherFreeTimes(teacherFreeTimeQuery, request, cancellationToken);

            methodResult.Result = new PagingItemsModel<TeacherFreeTimeModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        public async Task<IList<TeacherFreeTimeModel>> GetTeacherFreeTimes(IQueryable<TeacherFreeTimeModel> teacherFreeTimeQuery, SearchTeacherFreeTimeByCsoQuery request, CancellationToken cancellationToken)
        {
            var lists = await teacherFreeTimeQuery
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            var teacherResult = await _userService.GetTeacherByIdsAsync(new GetTeacherByIdsQueryModel { Ids = lists.Select(x => x.TeacherId).Distinct().ToList() });
            var teachers = teacherResult.Content?.Result;

            foreach (var item in lists)
            {
                var teacher = teachers?.FirstOrDefault(x => x.Id == item.TeacherId);
                item.TeacherName = teacher?.User?.FullName;
                item.TeacherCode = teacher?.User?.Code;
            }

            return lists;
        }
    }
}
