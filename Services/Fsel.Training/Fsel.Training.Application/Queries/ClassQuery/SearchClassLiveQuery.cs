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
    using Fsel.Course.Application.Services.UserServices.Models;
    using Fsel.Training.Application.Services.UserServices;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.EntityModels;
    using Fsel.Training.Domain.Models.QueryModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchClassLiveQuery : SearchClassLiveQueryModel, IRequest<MethodResult<PagingItemsModel<ClassModel>>>
    {
    }

    public class SearchClassLiveQueryHandler : IRequestHandler<SearchClassLiveQuery, MethodResult<PagingItemsModel<ClassModel>>>
    {
        private readonly IClassRepository _classRepository;
        private readonly IUserService _userService;

        public SearchClassLiveQueryHandler(IClassRepository classRepository, IUserService userService)
        {
            _classRepository = classRepository;
            _userService = userService;
        }

        public async Task<MethodResult<PagingItemsModel<ClassModel>>> Handle(SearchClassLiveQuery request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<PagingItemsModel<ClassModel>>();
            ArgumentNullException.ThrowIfNull(request);
            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var classLiveQuery = _classRepository.Queryable
                                        .Select(x => new ClassModel
                                        {
                                            Id = x.Id,
                                            StartTime = x.StartTime,
                                            EndTime = x.EndTime,
                                            TeacherId = x.TeacherId,
                                            CreatedDate = x.CreatedDate,
                                            Status = x.Status,
                                            Code = x.Code,
                                            CourseId = x.CourseId,
                                        });

            int totalItem = await classLiveQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await classLiveQuery
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            var teacherResult = await _userService.GetTeacherByIdsAsync(new GetTeacherByIdsQueryModel { Ids = classLiveQuery.Select(x => x.TeacherId ?? default).ToList() });
            var teachers = teacherResult.Content?.Result;

            foreach (var item in lists)
            {
                /*item.TeacherName = teachers.Content?.Result?.Where(x => item.TeacherId!.Contains(x.Id)).Select(x => x.Human?.FullName ?? string.Empty).ToList();*/
                //item.TeacherName = teachers.Where(x => item.TeacherId);
            }

            methodResult.Result = new PagingItemsModel<ClassModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
