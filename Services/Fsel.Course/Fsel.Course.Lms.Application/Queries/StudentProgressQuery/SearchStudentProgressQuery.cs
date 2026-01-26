// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.StudentProgressQuery
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.StudentProgress;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Services.ApplicationServices;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Course.Lms.Application.Services.UserServices.QueryModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;

    public class SearchStudentProgressQuery : SearchStudentProgressQueryModel, IRequest<MethodResult<PagingItemsModel<StudentProgressModel>>>
    {
    }

    public class SearchStudentProgressQueryHandler : IRequestHandler<SearchStudentProgressQuery, MethodResult<PagingItemsModel<StudentProgressModel>>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly ManagerProgressHelper _managerProgressHelper;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;
        private readonly IServiceProvider _serviceProvider;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILearningService _learningService;

        public SearchStudentProgressQueryHandler(ICourseRepository courseRepository,
            ICourseResultRepository courseResultRepository,
            ManagerProgressHelper managerProgressHelper,
            IUserService userService,
            AuthContext authContext,
            IServiceProvider serviceProvider,
            ICategoryRepository categoryRepository,
            ILearningService learningService)
        {
            _courseRepository = courseRepository;
            _courseResultRepository = courseResultRepository;
            _managerProgressHelper = managerProgressHelper;
            _userService = userService;
            _authContext = authContext;
            _serviceProvider = serviceProvider;
            _categoryRepository = categoryRepository;
            _learningService = learningService;
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

            long totalItem = default;

            var studentProgress = new List<StudentProgressModel>();

            var searchField = request.Serialize().Deserialize<SearchStudentsQueryModel>();

            if (searchField == null)
            {
                return methodResult;
            }

            searchField.ListProgram = request.ListProgram;
            searchField.ListSubject = request.ListSubject;
            searchField.ListLevel = request.ListLevel;

            var studentKeyResult = await _userService.SearchStudentAsync(searchField);

            var students = studentKeyResult.Content?.Result?.Items ?? new List<StudentSearchAdminModel>();
            totalItem = studentKeyResult.Content?.Result?.PagingInfo?.TotalItems ?? default;

            var subjects = await _categoryRepository.ReadQueryable.Where(p => p.Type == EnumTypeCategory.Subject).ToListAsync(cancellationToken);

            var learningTrees = await _learningService.GetLearningTreeFromCourseToTest(
                                     students.Where(p => p.CourseId.HasValue).Select(p => new GetLearningTreeFromCourseToTestModel
                                     {
                                         StudentId = p.Id,
                                         CourseId = p.CourseId ?? default,
                                     }).ToList(),
                                      cancellationToken);

            var tasks = students.Select(async student =>
            {
                using var rootScope = _serviceProvider.CreateScope();
                var learningService = rootScope.ServiceProvider.GetRequiredService<ILearningService>();

                var model = new StudentProgressModel
                {
                    StudentId = student.Id,
                    FullName = student.FullName,
                    Email = student.Email,
                    Level = student.CourseLevel ?? default,
                    CourseType = student.CourseLevel.GetEnumCourseType(),
                    CourseId = student.CourseId,
                    Subject = subjects.FirstOrDefault(p => p.Id == student.SubjectId)?.Name
                };

                if (student.CourseId.HasValue)
                {
                    var learningTree = learningTrees.FirstOrDefault(p => p.StudentId == student.Id);

                    if (learningTree != null)
                    {
                        var units = learningTree
                                            .GetAllItemByType<UnitComponentModel>().OrderBy(p => p.DisplayOrder)
                                            .ToList();

                        model.ContentProgress = $"{units?.SelectMany(p => p.Children).Sum(p => p.TotalContentCompleted)} / {units?.SelectMany(p => p.Children).Sum(p => p.TotalContent)}";
                        model.DisplayOrderUnit = units?.FirstOrDefault(p => p.Status == EnumResultStatus.New || p.Status == EnumResultStatus.Process)?.DisplayOrder;
                        model.DisplayOrderLesson = units?.SelectMany(p => p.Children).FirstOrDefault(p => p.Status == EnumResultStatus.New || p.Status == EnumResultStatus.Process)?.DisplayOrder;
                    }
                }

                return model;
            });

            studentProgress = (await Task.WhenAll(tasks)).ToList();

            methodResult.Result = new PagingItemsModel<StudentProgressModel>(studentProgress, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
