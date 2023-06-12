// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.ClassForumResultQuery
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.ClassForumResults;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchClassForumResultTeacherQuery : SearchClassForumResultTeacherQueryModel, IRequest<MethodResult<PagingItemsModel<ClassForumResultSearchModel>>>
    {
    }

    public class SearchClassForumResultTeacherQueryHandler : IRequestHandler<SearchClassForumResultTeacherQuery, MethodResult<PagingItemsModel<ClassForumResultSearchModel>>>
    {
        private readonly IClassForumResultRepository _classForumResultRepository;

        public SearchClassForumResultTeacherQueryHandler(IClassForumResultRepository classForumResultRepository)
        {
            _classForumResultRepository = classForumResultRepository;
        }

        public async Task<MethodResult<PagingItemsModel<ClassForumResultSearchModel>>> Handle(SearchClassForumResultTeacherQuery request, CancellationToken cancellationToken)
        {
            MethodResult<PagingItemsModel<ClassForumResultSearchModel>> methodResult = new MethodResult<PagingItemsModel<ClassForumResultSearchModel>>();
            ArgumentNullException.ThrowIfNull(request);
            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var classForumResultQuery = _classForumResultRepository.Queryable
                                    .Include(x => x.ClassForum)
                                    .ThenInclude(x => x!.Lesson)
                                    .ThenInclude(x => x!.UnitLessons.Where(n => !n.IsDeleted))
                                    .ThenInclude(x => x.Unit)
                                    .ThenInclude(x => x!.CourseUnitMockTests.Where(n => !n.IsDeleted))
                                    .ThenInclude(x => x.Course)
                                    .Select(x => new ClassForumResultSearchModel
                                    {
                                        Id = x.Id,
                                        CreatedFullName = x.CreatedFullName,
                                        CreatedDate = x.CreatedDate,
                                        Status = x.Status,
                                        CourseSkill = x.ClassForum!.CourseSkill,
                                        Courses = x.ClassForum.Lesson!.UnitLessons.Select(x => x.Unit).SelectMany(x => x.CourseUnitMockTests).Select(x => x.Course).Select(x => new CourseSearchModel
                                        {
                                            CourseType = x!.CourseType,
                                            CourseLevel = x!.CourseLevel,
                                        }).ToList(),
                                    });

            if (request.CourseLevel != null)
            {
                classForumResultQuery = classForumResultQuery.Where(m => m.Courses!.ToList().Select(x => x.CourseLevel).Contains(request.CourseLevel ?? default));
            }
            if (request.CourseType != null)
            {
                classForumResultQuery = classForumResultQuery.Where(m => m.CourseType == request.CourseType);
            }

            int totalItem = await classForumResultQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await classForumResultQuery
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            methodResult.Result = new PagingItemsModel<ClassForumResultSearchModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
