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
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;

        public SearchMockTestByTeacherQueryHandler(IMockTestResultRepository mockTestResultRepository, AuthContext authContext, IUserService userService)
        {
            _mockTestResultRepository = mockTestResultRepository;
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
            var mockTestResultQuery = _mockTestResultRepository.Queryable.Include(x => x.MockTest)
                                                                        .ThenInclude(x => x!.MockTestSections)
                                                                        .ThenInclude(x => x!.SectionGroup)
                                                                        .Include(x => x.Course)
                                                                        .ThenInclude(x => x!.CourseUnitMockTests)
                                                                        .Include(x => x.Unit)
                                                                        .ThenInclude(x => x!.UnitSkillMockTests)
                                                                        .Select(x => new MockTestResultSearchModel
                                                                        {
                                                                            Id = x.Id,
                                                                            CourseId = x.CourseId,
                                                                            UnitId = x.UnitId,
                                                                            CreatedDate = x.CreatedDate,
                                                                            CreatedFullName = x.CreatedFullName,
                                                                            CreatedUserId = x.CreatedUserId,
                                                                            Type = x.MockTest!.MockTestType,
                                                                            CourseSkills = x.MockTest.MockTestSections.Select(x => x.SectionGroup).Select(x => x!.CourseSkill).ToList()
                                                                        });

            //Keyword
            //if (!string.IsNullOrEmpty(request.Keyword))
            //{
            //    mockTestResultQuery = mockTestResultQuery.Where(m => m.Id.ToString() == request.Keyword || (m.Name ?? string.Empty).Contains(request.Keyword));
            //}



            if (request.MockTestFilter != null)
            {
                switch (request.MockTestFilter)
                {
                    case EnumMockTestFilter.Speaking:
                        break;
                    case EnumMockTestFilter.Writing:
                        break;
                    case EnumMockTestFilter.Full:
                        break;
                }
                mockTestResultQuery = mockTestResultQuery.Where(m => m.Type == EnumMockTestType.FullMockTest);
            }

            int totalItem = await mockTestResultQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await mockTestResultQuery
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            methodResult.Result = new PagingItemsModel<MockTestResultSearchModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
