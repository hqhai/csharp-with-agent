// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ClassForumResultQuery
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.ClassForumResults;
    using Fsel.Course.Lms.Application.Services.TrainingServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchClassForumResultQuery : SearchClassForumResultQueryModel, IRequest<MethodResult<PagingItemsModel<ClassForumResultSearchModel>>>
    {
    }

    public class SearchClassForumResultQueryHandler : IRequestHandler<SearchClassForumResultQuery, MethodResult<PagingItemsModel<ClassForumResultSearchModel>>>
    {
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly ITrainingService _trainingService;

        public SearchClassForumResultQueryHandler(IClassForumResultRepository classForumResultRepository
            , ITrainingService trainingService)
        {
            _classForumResultRepository = classForumResultRepository;
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
