// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.CourseQuery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.Courses;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetCourseProgressQuery : SearchCourseProgressQueryModel, IRequest<MethodResult<PagingItemsModel<CourseModel>>>
    {
    }

    public class GetCourseProgressQueryHandler : IRequestHandler<GetCourseProgressQuery, MethodResult<PagingItemsModel<CourseModel>>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly IFinalTestResultRepository _finalTestResultRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly IMapper _mapper;

        public GetCourseProgressQueryHandler(ICourseRepository courseRepository, IUnitResultRepository unitResultRepository, IFinalTestResultRepository finalTestResultRepository, IMockTestResultRepository mockTestResultRepository, IMapper mapper)
        {
            _courseRepository = courseRepository;
            _unitResultRepository = unitResultRepository;
            _finalTestResultRepository = finalTestResultRepository;
            _mockTestResultRepository = mockTestResultRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<PagingItemsModel<CourseModel>>> Handle(GetCourseProgressQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PagingItemsModel<CourseModel>> methodResult = new MethodResult<PagingItemsModel<CourseModel>>();

            var query = _courseRepository.Queryable.Include(x => x.CourseUnitMockTests)
                                                   .Include(x => x.CourseResults)
                                                   .Where(x => !x.ParentCourseId.HasValue)
                                                   .Where(x => x.CourseResults.Any() && x.CourseUnitMockTests.Any(x => !x.UnitId.HasValue && !x.MockTestId.HasValue && !x.FinalTestId.HasValue));

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                query = query.Where(m => m.Id.ToString() == request.Keyword || (m.Name ?? string.Empty).ToLower().Trim().Contains(request.Keyword.ToLower().Trim()));
            }
            if (request.CourseLevel != null)
            {
                query = query.Where(x => x.CourseLevel == request.CourseLevel);
            }

            var result = await _courseRepository.GetListByPageAsync<CourseModel>(query, request, cancellationToken);
            result.Items.ForEach(x =>
            {
                x.CourseUnitMockTests = GetCourseUnitMockTests(x.CourseUnitMockTests!.OrderBy(x => x.DisplayOrder).ToList(), cancellationToken).Result;
            });

            methodResult.Result = result;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<IList<CourseUnitMockTestModel>> GetCourseUnitMockTests(IList<CourseUnitMockTestModel> courseUnitMockTests, CancellationToken cancellationToken)
        {
            foreach (var item in courseUnitMockTests)
            {
                //Check điều kiện bản ghi hiện tại
                bool conditionSetStatus = await CheckConditionToSetStatus(item);
                if (string.IsNullOrEmpty(item.Type))
                {
                    item.IsUsed = null;
                }
                else if (conditionSetStatus)
                {
                    var courseUnitMockTestNext = courseUnitMockTests[courseUnitMockTests.IndexOf(item) + 1]; // Lấy bản ghi liền kề sau
                    item.IsUsed = courseUnitMockTestNext != null && !await CheckConditionToSetStatus(courseUnitMockTestNext); // Check điều kiện bản ghi liền kề sau
                }
                else
                {
                    item.IsUsed = false;
                }
            }
            return courseUnitMockTests;
        }

        private async Task<bool> CheckConditionToSetStatus(CourseUnitMockTestModel courseUnitMockTest)
        {
            bool isValid = false;

            if (courseUnitMockTest?.FinalTestId != null)
            {
                var finalTestResults = await _finalTestResultRepository.Queryable.AnyAsync(x => x.CourseId == courseUnitMockTest.CourseId && x.FinalTestId == courseUnitMockTest.FinalTestId && x.Status != EnumResultStatus.Unfinished);
                isValid = finalTestResults;
            }
            else if (courseUnitMockTest?.MockTest != null)
            {
                var mockTestResults = await _mockTestResultRepository.Queryable.AnyAsync(x => x.CourseId == courseUnitMockTest.CourseId && x.MockTestId == courseUnitMockTest.MockTestId && x.Status != EnumResultStatus.Unfinished);
                isValid = mockTestResults;
            }
            else if (courseUnitMockTest?.UnitId != null)
            {
                var unitResults = await _unitResultRepository.Queryable.AnyAsync(x => x.CourseId == courseUnitMockTest.CourseId && x.UnitId == courseUnitMockTest.UnitId && x.Status != EnumResultStatus.Unfinished);
                isValid = unitResults;
            }

            return isValid;
        }
    }
}
