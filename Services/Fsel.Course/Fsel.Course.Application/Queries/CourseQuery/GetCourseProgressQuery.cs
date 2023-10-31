// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.CourseQuery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
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

        public GetCourseProgressQueryHandler(ICourseRepository courseRepository, IUnitResultRepository unitResultRepository, IFinalTestResultRepository finalTestResultRepository, IMockTestResultRepository mockTestResultRepository)
        {
            _courseRepository = courseRepository;
            _unitResultRepository = unitResultRepository;
            _finalTestResultRepository = finalTestResultRepository;
            _mockTestResultRepository = mockTestResultRepository;
        }

        public async Task<MethodResult<PagingItemsModel<CourseModel>>> Handle(GetCourseProgressQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PagingItemsModel<CourseModel>> methodResult = new MethodResult<PagingItemsModel<CourseModel>>();

            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
            var query = _courseRepository.Queryable.Include(x => x.CourseUnitMockTests).Include(x => x.CourseResults).Where(x => x.CourseResults.Any() && x.CourseUnitMockTests.Any(x => !x.UnitId.HasValue && !x.MockTestId.HasValue && !x.FinalTestId.HasValue))
                .Select(x => new CourseModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Code = x.Code,
                    InstructionContent = x.InstructionContent,
                    Status = x.Status,
                    CourseLevel = x.CourseLevel,
                    CourseType = x.CourseType,
                    CreatedDate = x.CreatedDate,
                    CreatedFullName = x.CreatedFullName,
                    CreatedUserId = x.CreatedUserId,
                    UpdatedDate = x.UpdatedDate,
                    UpdatedUserId = x.UpdatedUserId,
                    UpdatedFullName = x.UpdatedFullName,
                    CourseUnitMockTests = x.CourseUnitMockTests.OrderBy(x => x.DisplayOrder).Select(o => new CourseUnitMockTestModel
                    {
                        CourseId = o.CourseId,
                        DisplayOrder = o.DisplayOrder,
                        UnitId = o.UnitId,
                        FinalTestId = o.FinalTestId,
                        MockTestId = o.MockTestId,
                        Type = o.FinalTestId != null ? nameof(o.FinalTest) : (o.MockTestId != null ? nameof(o.MockTest) : (o.UnitId != null ? nameof(o.Unit) : default)),
                    }).ToList()

                });
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                query = query.Where(m => m.Id.ToString() == request.Keyword || (m.Name ?? string.Empty).ToLower().Trim().Contains(request.Keyword.ToLower().Trim()));
            }
            if (request.CourseLevel != null)
            {
                query = query.Where(x => x.CourseLevel == request.CourseLevel);

            }

            int totalItem = await query.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

            var lists = await query
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            foreach (var item in lists)
            {
                if (item.CourseUnitMockTests != null)
                {
                    item.CourseUnitMockTests = await GetCourseUnitMockTests(item.CourseUnitMockTests);
                }
            }

            methodResult.Result = new PagingItemsModel<CourseModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<IList<CourseUnitMockTestModel>> GetCourseUnitMockTests(IList<CourseUnitMockTestModel> courseUnitMockTests)
        {
            foreach (var item in courseUnitMockTests)
            {
                if (string.IsNullOrEmpty(item.Type))
                {
                    item.Status = null;
                }
                else if (await CheckConditionAsync(item))
                {
                    var courseUnitMockTestNext = courseUnitMockTests[courseUnitMockTests.IndexOf(item) + 1]; 
                    item.Status = courseUnitMockTestNext != null && !await CheckConditionAsync(courseUnitMockTestNext);
                }
                else
                {
                    item.Status = false;
                }
            }
            return courseUnitMockTests;
        }


        private async Task<bool> CheckConditionAsync(CourseUnitMockTestModel courseUnitMockTest)
        {
            bool isValid = false;

            if (courseUnitMockTest?.FinalTestId != null)
            {
                var finalTestResults = await _finalTestResultRepository.Queryable.Where(x => x.CourseId == courseUnitMockTest.CourseId && x.FinalTestId == courseUnitMockTest.FinalTestId && x.Status != EnumResultStatus.Unfinished).ToListAsync();
                isValid =  finalTestResults.Any();

            }
            else if (courseUnitMockTest?.MockTest != null)
            {
                var mockTestResults = await _mockTestResultRepository.Queryable.Where(x => x.CourseId == courseUnitMockTest.CourseId && x.MockTestId == courseUnitMockTest.MockTestId && x.Status != EnumResultStatus.Unfinished).ToListAsync();
                isValid =  mockTestResults.Any();


            }
            else if (courseUnitMockTest?.UnitId != null)
            {
                var unitResults = await _unitResultRepository.Queryable.Where(x => x.CourseId == courseUnitMockTest.CourseId && x.UnitId == courseUnitMockTest.UnitId && x.Status != EnumResultStatus.Unfinished).ToListAsync();
                isValid = unitResults.Any();

            }

            return isValid;

        }
    }
}
