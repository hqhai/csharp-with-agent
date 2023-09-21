// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ClassForumResultQuery
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.ClassForumResults;
    using Fsel.Course.Lms.Application.Services.TrainingServices;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchClassForumResultByCsoQuery : SearchClassForumResultQueryModel, IRequest<MethodResult<PagingItemsModel<ClassForumResultSearchModel>>>
    {
    }

    public class SearchClassForumResultByCsoQueryHandler : IRequestHandler<SearchClassForumResultByCsoQuery, MethodResult<PagingItemsModel<ClassForumResultSearchModel>>>
    {
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly ITrainingService _trainingService;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;

        public SearchClassForumResultByCsoQueryHandler(IClassForumResultRepository classForumResultRepository, ITrainingService trainingService, AuthContext authContext, IUserService userService)
        {
            _classForumResultRepository = classForumResultRepository;
            _trainingService = trainingService;
            _authContext = authContext;
            _userService = userService;
        }

        public async Task<MethodResult<PagingItemsModel<ClassForumResultSearchModel>>> Handle(SearchClassForumResultByCsoQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PagingItemsModel<ClassForumResultSearchModel>> methodResult = new MethodResult<PagingItemsModel<ClassForumResultSearchModel>>();
            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
            var csoResults = await _userService.GetCSOByUserId(_authContext.CurrentUserId);
            var csoId = csoResults.Content?.Result?.Id;

            var classForumResultQuery = _classForumResultRepository.Queryable
                                    .Include(x => x.LessonResult)
                                    .ThenInclude(x => x!.Lesson)
                                    .ThenInclude(x => x!.UnitLessons)
                                    .ThenInclude(x => x.Unit)
                                    .ThenInclude(x => x!.CourseUnitMockTests)
                                    .Include(x => x.ClassForum)
                                    .Where(x => x.Status == EnumClassForumResultStatus.Pending && (x.CheckCsoId == null || x.CheckCsoId == csoId))
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
                                        Status = x.Status,
                                        CheckStartDate = x.CheckStartDate,
                                        CourseCode = x.LessonResult!.Course!.Code,
                                        LessonName = x.ClassForum!.Lesson!.Name,
                                        LessonDisplayOrder = x.LessonResult.Lesson!.UnitLessons.FirstOrDefault(y => y.UnitId == x.LessonResult.UnitId)!.DisplayOrder,
                                        UnitDisplayOrder = x.LessonResult.Unit!.CourseUnitMockTests.FirstOrDefault(y => y.CourseId == x.LessonResult.CourseId)!.DisplayOrder,
                                        UnitName = x.ClassForum.Lesson.UnitLessons.Select(x => x.Unit).Select(x => x!.Name).FirstOrDefault(),
                                        TeacherId = x.GradingTeacherId
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
            if (request.LessonDisplayOrder != null)
            {
                classForumResultQuery = classForumResultQuery.Where(m => m.LessonDisplayOrder == request.LessonDisplayOrder);
            }
            if (request.UnitDisplayOrder != null)
            {
                classForumResultQuery = classForumResultQuery.Where(m => m.UnitDisplayOrder == request.UnitDisplayOrder);
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
