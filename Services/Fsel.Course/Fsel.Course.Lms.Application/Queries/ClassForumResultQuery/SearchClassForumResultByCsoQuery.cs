// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ClassForumResultQuery
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.ClassForumResults;
    using Fsel.Course.Lms.Application.Services.TrainingServices;
    using Fsel.Course.Lms.Application.Services.TrainingServices.QueryModels;
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
        private readonly IMapper _mapper;

        public SearchClassForumResultByCsoQueryHandler(IClassForumResultRepository classForumResultRepository, ITrainingService trainingService, AuthContext authContext, IUserService userService, IMapper mapper)
        {
            _classForumResultRepository = classForumResultRepository;
            _trainingService = trainingService;
            _authContext = authContext;
            _userService = userService;
            _mapper = mapper;
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

            var classResults = await _trainingService.GetClassesByCsoIdAsync(csoId ?? default);
            var classModels = classResults.Content?.Result;
            var studentIds = classModels?.SelectMany(x => x.ClassStudents!).Select(x => x.StudentId).ToList();

            var classForumResultQuery = await _classForumResultRepository.Queryable
                                   .Include(x => x.LessonResult)
                                   .ThenInclude(x => x!.Lesson)
                                   .ThenInclude(x => x!.UnitLessons)
                                   .ThenInclude(x => x.Unit)
                                   .ThenInclude(x => x!.CourseUnitMockTests)
                                   .Include(x => x.ClassForum)
                                   .Where(x => x.Status == EnumClassForumResultStatus.Pending)
                                   .OrderByDescending(x => x.CreatedDate)
                                   .AsNoTracking()
                                   .Select(x => new ClassForumResultSearchModel
                                   {
                                       Id = x.Id,
                                       CreatedDate = x.CreatedDate,
                                       CreatedUserId = x.CreatedUserId,
                                       CreatedFullName = x.CreatedFullName,
                                       StudentId = x.StudentId,
                                       ClassForum = _mapper.Map<ClassForumModel>(x.ClassForum),
                                       Status = x.Status,
                                       CheckStartDate = x.CheckStartDate,
                                       CourseCode = x.LessonResult!.Course!.Code,
                                       LessonName = x.ClassForum!.Lesson!.Name,
                                       LessonDisplayOrder = x.LessonResult.Lesson!.UnitLessons.Where(y => y.UnitId == x.LessonResult.UnitId).Select(x => x.DisplayOrder).FirstOrDefault(),
                                       UnitDisplayOrder = x.LessonResult.Unit!.CourseUnitMockTests.Where(y => y.CourseId == x.LessonResult.CourseId).Select(x => x.Number).FirstOrDefault(),
                                       UnitName = x.ClassForum.Lesson.UnitLessons.Select(x => x.Unit).Select(x => x!.Name).FirstOrDefault(),
                                       TeacherId = x.GradingTeacherId,
                                       CourseId = x.LessonResult!.CourseId,
                                       WordContent = x.WordContent
                                   }).ToListAsync(cancellationToken);

            var userResults = await _userService.GetUsersByUserIdsAsync(classForumResultQuery.Select(x => x.CreatedUserId).ToList());
            var users = userResults?.Content?.Result;
            foreach (var item in classForumResultQuery)
            {
                item.CreatedFullName = users?.FirstOrDefault(x => x.Id == item.CreatedUserId)?.FullName ?? item.CreatedFullName;
            }
            var result = classForumResultQuery.AsEnumerable();
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                result = result.Where(m => m.Id.ToString() == request.Keyword || (m.CreatedFullName ?? string.Empty).ToLower().Trim().Contains(request.Keyword.ToLower().Trim()));
            }
            if (request.TeacherId != null)
            {
                result = result.Where(m => m.TeacherId == request.TeacherId);
            }
            if (request.CourseId != null)
            {
                result = result.Where(m => m.CourseId == request.CourseId);
            }
            if (request.LessonName != null)
            {
                result = result.Where(m => (m.LessonName ?? string.Empty).ToLower().Trim().Contains(request.LessonName.ToLower().Trim()));
            }
            if (request.UnitName != null)
            {
                result = result.Where(m => (m.UnitName ?? string.Empty).ToLower().Trim().Contains(request.UnitName.ToLower().Trim()));
            }
            if (request.LessonDisplayOrder != null)
            {
                result = result.Where(m => m.LessonDisplayOrder == request.LessonDisplayOrder);
            }
            if (request.UnitDisplayOrder != null)
            {
                result = result.Where(m => m.UnitDisplayOrder == request.UnitDisplayOrder);
            }

            int totalItem = result.Count();
            var lists = result.ApplySortAndPaging(request).ToList();

            var classToCourseResult = await _trainingService.GetsByCourseIdsAsync(new GetsByCourseIdsQueryModel { CourseIdStr = string.Join(",", lists.Select(x => x.CourseId).Distinct().ToList()) });
            var classResultModel = classToCourseResult.Content?.Result;
            foreach (var item in lists)
            {
                var classModel = classResultModel?.FirstOrDefault(x => x.CourseId == item.CourseId);
                if (classModel != null)
                {
                    item.ClassCode = classModel.Code;
                }
                item.PostArea = "L" + item.LessonDisplayOrder + "_" + "U" + item.UnitDisplayOrder + "_" + item.CourseCode;
            }

            methodResult.Result = new PagingItemsModel<ClassForumResultSearchModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
