// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.TeacherFreeDateQuery
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Training.Application.Services.UserServices;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.EntityModels;
    using Fsel.Training.Domain.Models.QueryModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchTeacherFreeDateQuery : SearchTeacherFreeDateQueryModel, IRequest<MethodResult<PagingItemsModel<TeacherFreeDateModel>>>
    {
    }

    public class SearchTeacherFreeDateQueryHandler : IRequestHandler<SearchTeacherFreeDateQuery, MethodResult<PagingItemsModel<TeacherFreeDateModel>>>
    {
        private readonly ITeacherFreeDateRepository _teacherFreeDateRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;

        public SearchTeacherFreeDateQueryHandler(ITeacherFreeDateRepository teacherFreeDateRepository, AuthContext authContext, IUserService userService)
        {
            _teacherFreeDateRepository = teacherFreeDateRepository;
            _authContext = authContext;
            _userService = userService;
        }

        public async Task<MethodResult<PagingItemsModel<TeacherFreeDateModel>>> Handle(SearchTeacherFreeDateQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            MethodResult<PagingItemsModel<TeacherFreeDateModel>> methodResult = new MethodResult<PagingItemsModel<TeacherFreeDateModel>>();
            var teacher = await _userService.GetTeacherByIdAsync(_authContext.CurrentUserId);
            var teacherId = teacher.Content?.Result?.Id;

            var teacherFreeDateQuery = _teacherFreeDateRepository.Queryable.Where(x => x.TeacherId == teacherId)
                                    .Select(x => new TeacherFreeDateModel
                                    {
                                        Id = x.Id,
                                        StartTime = x.StartTime,
                                        EndTime = x.EndTime,
                                        TeacherId = x.TeacherId,
                                        CreatedDate = x.CreatedDate,
                                    });
            int totalItem = await teacherFreeDateQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await teacherFreeDateQuery
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            methodResult.Result = new PagingItemsModel<TeacherFreeDateModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
