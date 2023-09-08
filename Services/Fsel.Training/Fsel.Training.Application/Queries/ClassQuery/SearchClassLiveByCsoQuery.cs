// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.ClassQuery
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

    public class SearchClassLiveByCsoQuery : SearchClassLiveByCsoQueryModel, IRequest<MethodResult<PagingItemsModel<ClassLiveCalendarModel>>>
    {
    }

    public class SearchClassLiveByCsoQueryHandler : IRequestHandler<SearchClassLiveByCsoQuery, MethodResult<PagingItemsModel<ClassLiveCalendarModel>>>
    {
        private readonly IClassLiveCalendarRepository _classLiveCalendarRepository;
        private readonly IUserService _userService;

        public SearchClassLiveByCsoQueryHandler(IClassLiveCalendarRepository classLiveCalendarRepository, IUserService userService)
        {
            _classLiveCalendarRepository = classLiveCalendarRepository;
            _userService = userService;
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

            var classLiveQuery = _classLiveCalendarRepository.Queryable
                                        .Include(x => x.Class)
                                        .Select(x => new ClassLiveCalendarModel
                                        {
                                            Id = x.Id,
                                            ClassId = x.ClassId,
                                            Class = new ClassModel
                                            {
                                                Id = x.Class!.Id,
                                                Name = x.Class!.Name,
                                                Code = x.Class!.Code,
                                                TeacherId = x.Class!.TeacherId,
                                                StartDate = x.Class!.StartDate.HasValue ? x.Class!.StartDate.Value : default,
                                                EndDate = x.Class!.EndDate.HasValue ? x.Class!.EndDate.Value : default,
                                                LiveDays = x.Class!.LiveDays,
                                            },
                                            CreatedDate = x.CreatedDate
                                        });
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                classLiveQuery = classLiveQuery.Where(m => m.Id.ToString() == request.Keyword || (m.Class!.Name ?? string.Empty).Contains(request.Keyword));
            }

            if (request.ClassCode != null)
            {
                classLiveQuery = classLiveQuery.Where(m => m.Class!.Code == request.ClassCode);
            }
            int totalItem = await classLiveQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await classLiveQuery
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            var teacherResult = await _userService.GetTeacherByIdsAsync(new GetTeacherByIdsQueryModel { Ids = lists.Select(x => x.Class!.TeacherId ?? default).Distinct().ToList() });
            var teachers = teacherResult.Content?.Result;

            foreach (var item in lists)
            {
                var teacher = teachers!.FirstOrDefault(x => x.Id == item.Class!.TeacherId);
                item.Class!.TeacherName = teacher?.Human?.FullName;
            }

            methodResult.Result = new PagingItemsModel<ClassLiveCalendarModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
