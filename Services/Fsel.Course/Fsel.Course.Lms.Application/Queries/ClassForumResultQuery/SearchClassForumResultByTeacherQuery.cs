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
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchClassForumResultByTeacherQuery : SearchClassForumResultQueryModel, IRequest<MethodResult<PagingItemsModel<ClassForumResultSearchModel>>>
    {
    }

    public class SearchClassForumResultByTeacherQueryHandler : IRequestHandler<SearchClassForumResultByTeacherQuery, MethodResult<PagingItemsModel<ClassForumResultSearchModel>>>
    {
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly ITrainingService _trainingService;
        private readonly AuthContext _authContext;

        public SearchClassForumResultByTeacherQueryHandler(IClassForumResultRepository classForumResultRepository, ITrainingService trainingService, AuthContext authContext)
        {
            _classForumResultRepository = classForumResultRepository;
            _trainingService = trainingService;
            _authContext = authContext;
        }

        public async Task<MethodResult<PagingItemsModel<ClassForumResultSearchModel>>> Handle(SearchClassForumResultByTeacherQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PagingItemsModel<ClassForumResultSearchModel>> methodResult = new MethodResult<PagingItemsModel<ClassForumResultSearchModel>>();
            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var classForumResultQuery = _classForumResultRepository.Queryable
                                    .Include(x => x.LessonResult)
                                    .ThenInclude(x => x!.Lesson)
                                    .ThenInclude(x => x!.UnitLessons)
                                    .ThenInclude(x => x.Unit)
                                    .ThenInclude(x => x!.CourseUnitMockTests)
                                    .ThenInclude(x => x.Course)
                                    .Include(x => x.ClassForum)
                                    .Where(x => x.Status == EnumClassForumResultStatus.PendingForGrading && (x.GradingTeacherId == null || x.GradingTeacherId == _authContext.CurrentUserId))
                                    .Select(x => new ClassForumResultSearchModel
                                    {
                                        Id = x.Id,
                                        CreatedDate = x.CreatedDate,
                                        CreatedUserId = x.CreatedUserId,
                                        CreatedFullName = x.CreatedFullName,
                                        CourseSkill = x.ClassForum!.CourseSkill,
                                        CourseId = x.LessonResult!.CourseId,
                                        ClassForum = x.ClassForum!.ClassForumResults!.Select(x => x.ClassForum).Select(x => new ClassForumModel
                                        {
                                            Id = x!.Id,
                                            CreatedUserId = x.CreatedUserId,
                                            CreatedFullName = x.CreatedFullName,
                                            CourseSkill = x.CourseSkill,
                                        }).FirstOrDefault(),
                                        Status = x.Status,
                                        GradingStartDate = x.GradingStartDate,
                                        CourseCode = x.LessonResult!.Course!.Code,
                                        LessonName = x.ClassForum!.Lesson!.Name,
                                        LessonDisplayOrder = x.LessonResult.Lesson!.UnitLessons.Where(y => y.UnitId == x.LessonResult.UnitId).Select(x => x.DisplayOrder).FirstOrDefault(),
                                        UnitDisplayOrder = x.LessonResult.Unit!.CourseUnitMockTests.Where(y => y.CourseId == x.LessonResult.CourseId).Select(x => x.Number).FirstOrDefault(),
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
            if (request.CourseId != null)
            {
                classForumResultQuery = classForumResultQuery.Where(m => m.CourseId == request.CourseId);
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
                item.PostArea = "L" + item.LessonDisplayOrder + "_" + "U" + item.UnitDisplayOrder + "_" + item.CourseCode;
            }

            methodResult.Result = new PagingItemsModel<ClassForumResultSearchModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
