// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ClassForumResultQuery
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.ClassForumResults;
    using Fsel.Course.Lms.Application.Services.TrainingServices;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchClassForumResultQuery : SearchClassForumResultQueryModel, IRequest<MethodResult<PagingItemsModel<ClassForumResultSearchModel>>>
    {
    }

    public class SearchClassForumResultQueryHandler : IRequestHandler<SearchClassForumResultQuery, MethodResult<PagingItemsModel<ClassForumResultSearchModel>>>
    {
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly IClassForumRepository _classForumRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;
        private readonly ITrainingService _trainingService;

        public SearchClassForumResultQueryHandler(IClassForumResultRepository classForumResultRepository
            , IClassForumRepository classForumRepository
            , ILessonRepository lessonRepository
            , IUserService userService
            , AuthContext authContext
            , ITrainingService trainingService)
        {
            _classForumResultRepository = classForumResultRepository;
            _classForumRepository = classForumRepository;
            _lessonRepository = lessonRepository;
            _userService = userService;
            _authContext = authContext;
            _trainingService = trainingService;
        }

        public async Task<MethodResult<PagingItemsModel<ClassForumResultSearchModel>>> Handle(SearchClassForumResultQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PagingItemsModel<ClassForumResultSearchModel>> methodResult = new MethodResult<PagingItemsModel<ClassForumResultSearchModel>>();
            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var classForumResultQuery = _classForumResultRepository.Queryable
                                    .Include(x => x.ClassForum)
                                    .ThenInclude(x => x!.Lesson)
                                    .ThenInclude(x => x!.UnitLessons.Where(y => y.Unit != null))
                                    .ThenInclude(x => x.Unit)
                                    .ThenInclude(x => x!.CourseUnitMockTests)
                                    .ThenInclude(x => x.Course)
                                    .ThenInclude(x => x!.CourseTeachers.Where(y => y.Course != null))
                                    .Where(x => x.Status == request.Status)
                                    .Select(x => new ClassForumResultSearchModel
                                    {
                                        Id = x.Id,
                                        CreatedDate = x.CreatedDate,
                                        CreatedUserId = x.CreatedUserId,
                                        CreatedFullName = x.CreatedFullName,
                                        ClassForum = x.ClassForum!.ClassForumResults!.Select(x => x.ClassForum).Select(x => new ClassForumModel
                                        {
                                            Id = x!.Id,
                                            CreatedUserId = x.CreatedUserId,
                                            CreatedFullName = x.CreatedFullName,
                                        }).FirstOrDefault(),
                                        LessonName = x.ClassForum!.Lesson!.Name,
                                        UnitName = x.ClassForum.Lesson.UnitLessons.Select(x => x.Unit).Select(x => x!.Name).FirstOrDefault(),
                                        TeacherId = x.ClassForum.Lesson.UnitLessons
                                                            .Select(x => x.Unit)
                                                            .SelectMany(x => x!.CourseUnitMockTests)
                                                            .Select(x => x.Course)
                                                            .SelectMany(x => x!.CourseTeachers)
                                                            .Select(x => x.TeacherId).FirstOrDefault(),
                                    });

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                classForumResultQuery = classForumResultQuery.Where(m => m.Id.ToString() == request.Keyword || (m.CreatedFullName ?? string.Empty).Contains(request.Keyword));
            }
            if (request.TeacherId != null)
            {
                classForumResultQuery = classForumResultQuery.Where(m => m.TeacherId == request.TeacherId);
            }
            if (request.LessonName != null)
            {
                classForumResultQuery = classForumResultQuery.Where(m => (m.LessonName ?? string.Empty).Contains(request.LessonName));
            }
            if (request.UnitName != null)
            {
                classForumResultQuery = classForumResultQuery.Where(m => (m.UnitName ?? string.Empty).Contains(request.UnitName));
            }
            int totalItem = await classForumResultQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await classForumResultQuery
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            foreach (var item in lists)
            {
                var studentId = item.CreatedUserId;
                var classResult = await _trainingService.GetClassByStudentId(studentId);
                if (classResult!.Content!.Result != null)
                {
                    item.ClassCode = classResult!.Content!.Result.Code;
                }
            }

            methodResult.Result = new PagingItemsModel<ClassForumResultSearchModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
