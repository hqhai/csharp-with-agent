// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.MockTestResultQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.MockTests;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchMockTestByTeacherQuery : SearchMockTestByTeacherQueryModel, IRequest<MethodResult<PagingItemsModel<MockTestResultSearchModel>>>
    {
    }

    public class SearchMockTestByTeacherQueryHandler : IRequestHandler<SearchMockTestByTeacherQuery, MethodResult<PagingItemsModel<MockTestResultSearchModel>>>
    {
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;

        public SearchMockTestByTeacherQueryHandler(IMockTestResultRepository mockTestResultRepository, ICourseRepository courseRepository, AuthContext authContext, IUserService userService)
        {
            _mockTestResultRepository = mockTestResultRepository;
            _courseRepository = courseRepository;
            _authContext = authContext;
            _userService = userService;
        }

        public async Task<MethodResult<PagingItemsModel<MockTestResultSearchModel>>> Handle(SearchMockTestByTeacherQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<MockTestResultSearchModel>>();

            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var teacherResult = await _userService.GetTeacherByUserIdAsync(_authContext.CurrentUserId);
            var teacherId = teacherResult.Content?.Result?.Id;

            var mockTestResultQuery = _mockTestResultRepository.Queryable.Include(x => x.MockTest)
                                                                        .ThenInclude(x => x!.MockTestSections)
                                                                        .ThenInclude(x => x!.SectionGroup)
                                                                        .Include(x => x.MockTestScores)
                                                                        .Where(x => x.Status == EnumResultStatus.Done && x.MockTestScores.Count == 0 && (x.GradingTeacherId == null || x.GradingTeacherId == teacherId))
                                                                        .AsNoTracking()
                                                                        .Select(x => new MockTestResultSearchModel
                                                                        {
                                                                            Id = x.Id,
                                                                            CourseId = x.CourseId,
                                                                            UnitId = x.UnitId,
                                                                            MockTestId = x.MockTestId,
                                                                            CreatedDate = x.CreatedDate,
                                                                            CreatedFullName = x.CreatedFullName,
                                                                            CreatedUserId = x.CreatedUserId,
                                                                            Type = x.MockTest!.MockTestType,
                                                                            CourseSkill = x.MockTest.MockTestSections.Select(x => x.SectionGroup).Select(x => x!.CourseSkill).FirstOrDefault(),
                                                                        });
            mockTestResultQuery = mockTestResultQuery.Where(x => x.CourseSkill == EnumCourseSkill.Speaking || x.CourseSkill == EnumCourseSkill.Writing || x.Type == EnumMockTestType.FullMockTest);
            //Keyword
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                mockTestResultQuery = mockTestResultQuery.Where(m => m.Id.ToString() == request.Keyword || (m.CreatedFullName ?? string.Empty).Contains(request.Keyword));
            }

            if (request.MockTestFilter != null)
            {
                switch (request.MockTestFilter)
                {
                    case EnumMockTestFilter.Speaking:
                        mockTestResultQuery = mockTestResultQuery.Where(m => m.Type == EnumMockTestType.SkillMockTest && m.CourseSkill == EnumCourseSkill.Speaking);
                        break;

                    case EnumMockTestFilter.Writing:
                        mockTestResultQuery = mockTestResultQuery.Where(m => m.Type == EnumMockTestType.SkillMockTest && m.CourseSkill == EnumCourseSkill.Writing);
                        break;

                    case EnumMockTestFilter.Full:
                        mockTestResultQuery = mockTestResultQuery.Where(m => m.Type == EnumMockTestType.FullMockTest);
                        break;
                }
            }
            if (request.CourseIds != null && request.CourseIds.Count > 0)
            {
                mockTestResultQuery = mockTestResultQuery.Where(m => request.CourseIds.Contains(m.CourseId));
            }

            int totalItem = await mockTestResultQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await mockTestResultQuery
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            var courses = await _courseRepository.Queryable.Include(x => x.CourseUnitMockTests)
                                                        .ThenInclude(x => x.Unit)
                                                        .ThenInclude(x => x!.UnitSkillMockTests)
                                                        .Include(x => x.CourseUnitMockTests)
                                                        .Where(x => lists.Select(y => y.CourseId).Contains(x.Id))
                                                        .ToListAsync(cancellationToken: cancellationToken);
            foreach (var item in lists)
            {
                var course = courses.FirstOrDefault(x => x.Id == item.CourseId);
                if (course != null)
                {
                    item.CourseName = course.Name;
                    if (item.Type == EnumMockTestType.SkillMockTest)
                    {
                        var courseUnitMockTest = course.CourseUnitMockTests.FirstOrDefault(x => x.Unit != null && x!.UnitId == item.UnitId);
                        if (courseUnitMockTest != null && courseUnitMockTest.Unit != null && courseUnitMockTest.Unit.UnitSkillMockTests.FirstOrDefault(x => x.MockTestId == item.MockTestId) != null)
                        {
                            item.UnitName = courseUnitMockTest.Unit.Name;
                            item.PostArea = "U" + courseUnitMockTest.DisplayOrder + "_" + course.Name;
                        }
                    }
                    else
                    {
                        var number = course.CourseUnitMockTests.FirstOrDefault(x => x.MockTestId == item.MockTestId)?.DisplayOrder;
                        item.PostArea = "FM" + number + "_" + course.Name;
                    }
                }
            }

            methodResult.Result = new PagingItemsModel<MockTestResultSearchModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
