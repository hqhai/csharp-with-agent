// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.StudentProgressQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.ManageProgress;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchStudentStudentProgressQuery : SearchManageStudentProgressQueryModel, IRequest<MethodResult<PagingItemsModel<StudentManageProgressModel>>>
    {
    }

    public class SearchManageStudentProgressQueryHandler : IRequestHandler<SearchStudentStudentProgressQuery, MethodResult<PagingItemsModel<StudentManageProgressModel>>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly IUserService _userService;

        public SearchManageStudentProgressQueryHandler(ICourseRepository courseRepository,
            ICourseResultRepository courseResultRepository,
            IUserService userService)
        {
            _courseRepository = courseRepository;
            _courseResultRepository = courseResultRepository;
            _userService = userService;
        }

        public async Task<MethodResult<PagingItemsModel<StudentManageProgressModel>>> Handle(SearchStudentStudentProgressQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<StudentManageProgressModel>>();

            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
            var sortedResults = await _courseResultRepository.Queryable
                .Where(cr => !cr.IsDeleted)
                .OrderBy(cr => cr.CreatedDate)
                .ToListAsync(cancellationToken);

            var sortedCourses = await _courseRepository.Queryable
                .Where(c => !c.IsDeleted)
                .ToListAsync(cancellationToken);

            var courseResults = sortedResults
                .GroupBy(r => new { r.StudentId, r.CourseId })
                .Select(group => new
                {
                    StudentId = group.Key.StudentId,
                    CourseId = group.Key.CourseId,
                    CourseType = sortedCourses.FirstOrDefault(c => c.Id == group.Key.CourseId)?.CourseType ?? default,
                    MaxCreatedDate = group.Max(r => r.CreatedDate)
                })
                .OrderByDescending(x => x.MaxCreatedDate)
                .ToList();

            //var courseResults = query.AsEnumerable().ToList();

            //var courseResults = await query.ToListAsync(cancellationToken);
            var studentResults = await _userService.GetStudentsByStudentIdsAsync(courseResults.Select(x => x.StudentId).ToList());
            var students = studentResults.Content?.Result;
            var manageStudents = new List<StudentManageProgressModel>();
            foreach (var courseResult in courseResults)
            {
                StudentManageProgressModel manageStudentProgressModel = new StudentManageProgressModel();
                if (students != null && students.Any())
                {
                    var student = students.FirstOrDefault(x => x.Id == courseResult.StudentId);
                    manageStudentProgressModel.StudentId = courseResult.StudentId;
                    manageStudentProgressModel.FullName = student?.Human?.FullName;
                }
                manageStudentProgressModel.CourseType = courseResult.CourseType;
                manageStudentProgressModel.CourseId = courseResult.CourseId;

                manageStudents.Add(manageStudentProgressModel);
            }
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                manageStudents = manageStudents.Where(m => (m.FullName ?? string.Empty).Contains(request.Keyword)).ToList();
            }

            int totalItem = manageStudents.Count;
            var lists = manageStudents.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToList();
            foreach (var item in lists)
            {
                var (currentProgress, progress, displayOrderUnit, displayOrderLesson) = await _courseRepository.GetContentCompleted(item.CourseId, item.CourseType, item.StudentId);
                item.DisplayOrderLesson = displayOrderLesson;
                item.DisplayOrderUnit = displayOrderUnit;
                item.ContentProgress = string.Format("{0} / {1}", currentProgress, progress);
            }
            methodResult.Result = new PagingItemsModel<StudentManageProgressModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
