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

            var query = _mockTestResultRepository.Queryable.Include(x => x.MockTest)
                                                                        .ThenInclude(x => x!.MockTestSections)
                                                                        .ThenInclude(x => x!.SectionGroup)
                                                                        .Include(x => x.MockTest)
                                                                        .ThenInclude(x => x!.CourseUnitMockTests)
                                                                        .ThenInclude(x => x.Course)
                                                                        .Include(x => x.MockTest)
                                                                        .ThenInclude(x => x!.UnitSkillMockTests)
                                                                        .ThenInclude(x => x.Unit)
                                                                        .ThenInclude(x => x.CourseUnitMockTests)
                                                                        .ThenInclude(x => x.Course)
                                                                        .Include(x => x.MockTestScores)
                                                                         //.Where(x => x.Status == EnumResultStatus.Done && !x.MockTestScores.Any && (x.GradingTeacherId == null || x.GradingTeacherId == teacherId))
                                                                         .Where(x => x.Status == EnumResultStatus.Done && x.MockTestScores.Count < 4 && (x.GradingTeacherId == null || x.GradingTeacherId == teacherId))  // sử dụng cho phiên bản chấm điểm bằng teacher và AI
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
                                                                            UnitDisplayOrder = x.MockTest.MockTestType == EnumMockTestType.FullMockTest ? x.MockTest.CourseUnitMockTests.Select(x => x.Number).FirstOrDefault() : x.MockTest.UnitSkillMockTests.Where(y => y.UnitId == x.UnitId).Select(x => x.Unit).SelectMany(x => x.CourseUnitMockTests).Where(y => y.CourseId == x.CourseId).Select(x => x.Number).FirstOrDefault(),
                                                                            CourseCode = x.MockTest.MockTestResults.Where(y => y.Id == x.Id).Select(x => x.Course!.Code).FirstOrDefault(),
                                                                        });
            var mockTestResultQuery = await query.Where(x => x.CourseSkill == EnumCourseSkill.Speaking || x.Type == EnumMockTestType.FullMockTest).ToListAsync(cancellationToken);
            //Keyword

            var userResults = await _userService.GetUsersByUserIdsAsync(mockTestResultQuery.Select(x => x.CreatedUserId).ToList());
            var users = userResults?.Content?.Result;

            foreach (var item in mockTestResultQuery)
            {
                item.CreatedFullName = users?.FirstOrDefault(x => x.Id == item.CreatedUserId)?.FullName ?? item.CreatedFullName;
            }
            var result = mockTestResultQuery.AsEnumerable();
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                result = result.Where(m => m.Id.ToString() == request.Keyword || (m.CreatedFullName ?? string.Empty).ToLower().Trim().Contains(request.Keyword.ToLower().Trim()));
            }

            if (request.MockTestFilter != null)
            {
                switch (request.MockTestFilter)
                {
                    case EnumMockTestFilter.Speaking:
                        result = result.Where(m => m.Type == EnumMockTestType.SkillMockTest && m.CourseSkill == EnumCourseSkill.Speaking);
                        break;

                    case EnumMockTestFilter.Full:
                        result = result.Where(m => m.Type == EnumMockTestType.FullMockTest);
                        break;
                }
            }

            if (request.UnitDisplayOrder != null)
            {
                result = result.Where(m => m.UnitDisplayOrder == request.UnitDisplayOrder);
            }
            if (request.CourseId != null)
            {
                result = result.Where(m => m.CourseId == request.CourseId);
            }

            int totalItem = result.Count();
            var lists = result.ApplySortAndPaging(request).ToList();

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
