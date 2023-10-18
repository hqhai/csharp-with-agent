// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.MockTestResultQuery
{
    using System.Globalization;
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
                                                                        .Include(x => x.MockTest)
                                                                        .ThenInclude(x => x!.MockTestResults)
                                                                        .ThenInclude(x => x.Course)
                                                                        .ThenInclude(x => x!.CourseUnitMockTests)
                                                                        .Include(x => x.MockTestScores)
                                                                        .Where(x => x.Status == EnumResultStatus.Done && !x.MockTestScores.Any() && (x.GradingTeacherId == null || x.GradingTeacherId == teacherId))
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
                                                                            UnitDisplayOrder = x.MockTest.CourseUnitMockTests.Select(x => x.Number).FirstOrDefault(),
                                                                            CourseCode = x.MockTest.MockTestResults.Select(x => x.Course!.Code).FirstOrDefault(),
                                                                        });
            mockTestResultQuery = mockTestResultQuery.Where(x => x.CourseSkill == EnumCourseSkill.Speaking || x.CourseSkill == EnumCourseSkill.Writing || x.Type == EnumMockTestType.FullMockTest);
            //Keyword
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                mockTestResultQuery = mockTestResultQuery.Where(m => m.Id.ToString() == request.Keyword || (m.CreatedFullName ?? string.Empty).ToLower().Trim().Contains(request.Keyword.ToLower().Trim()));
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

            if (request.UnitDisplayOrder != null)
            {
                mockTestResultQuery = mockTestResultQuery.Where(m => m.UnitDisplayOrder == request.UnitDisplayOrder);
            }
            if (request.CourseId != null)
            {
                mockTestResultQuery = mockTestResultQuery.Where(m => m.CourseId == request.CourseId);
            }

            int totalItem = await mockTestResultQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await mockTestResultQuery
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            foreach (var item in lists)
            {
                if (item.Type == EnumMockTestType.SkillMockTest)
                {
                    item.PostArea = "U" + item.UnitDisplayOrder + "_" + item.CourseCode;
                }
                else
                {
                    item.PostArea = "FM" + item.UnitDisplayOrder + "_" + item.CourseCode;
                }
            }

            methodResult.Result = new PagingItemsModel<MockTestResultSearchModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
