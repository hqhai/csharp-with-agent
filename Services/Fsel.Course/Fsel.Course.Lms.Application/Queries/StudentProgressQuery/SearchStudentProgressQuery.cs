// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.StudentProgressQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.StudentProgress;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchStudentProgressQuery : SearchStudentProgressQueryModel, IRequest<MethodResult<PagingItemsModel<StudentProgressModel>>>
    {
    }

    public class SearchStudentProgressQueryHandler : IRequestHandler<SearchStudentProgressQuery, MethodResult<PagingItemsModel<StudentProgressModel>>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly IUserService _userService;

        public SearchStudentProgressQueryHandler(ICourseRepository courseRepository,
            ICourseResultRepository courseResultRepository,
            IUserService userService)
        {
            _courseRepository = courseRepository;
            _courseResultRepository = courseResultRepository;
            _userService = userService;
        }

        public async Task<MethodResult<PagingItemsModel<StudentProgressModel>>> Handle(SearchStudentProgressQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<StudentProgressModel>>();

            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
            var courseResults = await _courseResultRepository.Queryable.Include(x => x.Course)
                .GroupBy(r => new { r.StudentId, r.CourseId })
                .Select(group => new CourseResultModel
                {
                    StudentId = group.Key.StudentId,
                    CourseId = group.Key.CourseId,
                    CourseType = group.Select(x => x.Course).FirstOrDefault(c => c!.Id == group.Key.CourseId)!.CourseType,
                    CourseLevel = group.Select(x => x.Course).FirstOrDefault(c => c!.Id == group.Key.CourseId)!.CourseLevel,
                    CreatedDate = group.Max(r => r.CreatedDate)
                })
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync(cancellationToken);

            var studentResults = await _userService.GetStudentsByStudentIdsAsync(courseResults.Select(x => x.StudentId).ToList());
            var students = studentResults.Content?.Result;
            var studentProgress = new List<StudentProgressModel>();
            foreach (var courseResult in courseResults)
            {
                StudentProgressModel studentProgressModel = new StudentProgressModel();
                if (students != null && students.Any())
                {
                    var student = students.FirstOrDefault(x => x.Id == courseResult.StudentId);
                    studentProgressModel.StudentId = courseResult.StudentId;
                    studentProgressModel.FullName = student?.Human?.FullName;
                }
                studentProgressModel.Level = courseResult.CourseLevel ?? default;
                studentProgressModel.CourseType = courseResult.CourseType ?? default;
                studentProgressModel.CourseId = courseResult.CourseId;
                studentProgress.Add(studentProgressModel);
            }
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                studentProgress = studentProgress.Where(m => (m.FullName ?? string.Empty).Contains(request.Keyword)).ToList();
            }

            if (request.CourseType != null)
            {
                studentProgress = studentProgress.Where(m => m.CourseType == request.CourseType).ToList();
            }

            if (request.Level != null)
            {
                studentProgress = studentProgress.Where(m => m.Level == request.Level).ToList();
            }

            int totalItem = studentProgress.Count;
            var lists = studentProgress.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToList();
            foreach (var item in lists)
            {
                var courseResult = courseResults.FirstOrDefault(x => x.CourseId == item.CourseId && x.StudentId == item.StudentId);
                if (courseResult != null)
                {
                    var (currentProgress, progress) = await _courseRepository.GetContentComplete(courseResult);
                    var (displayOrderUnit, displayOrderLesson) = await _courseRepository.GetDisplayOrder(courseResult);
                    item.DisplayOrderLesson = displayOrderLesson;
                    item.DisplayOrderUnit = displayOrderUnit;
                    item.ContentProgress = string.Format("{0} / {1}", currentProgress, progress);
                }
            }
            methodResult.Result = new PagingItemsModel<StudentProgressModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
