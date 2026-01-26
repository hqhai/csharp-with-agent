// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.LeaderBoardCollectiveAwardQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.QueryModels.LeaderBoardRewards;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetLeaderBoardCollectiveQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<LeaderBoardCollectiveAwardModel>>>
    {
        public string? EventCode { get; set; }
    }

    public class GetLeaderBoardCollectiveQueryHandler : IRequestHandler<GetLeaderBoardCollectiveQuery, MethodResult<PagingItemsModel<LeaderBoardCollectiveAwardModel>>>
    {
        private readonly IUserService _userService;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly IPlacementTestGroupResultRepository _placementTestGroupResultRepository;
        private readonly ISystemService _systemService;
        private readonly ICourseUnitMockTestRepository _courseUnitMockTestRepository;

        public GetLeaderBoardCollectiveQueryHandler(IUserService userService,
                                                    ICourseResultRepository courseResultRepository,
                                                    IUnitResultRepository unitResultRepository,
                                                    IPlacementTestGroupResultRepository placementTestGroupResultRepository,
                                                    ISystemService systemService,
                                                    ICourseUnitMockTestRepository courseUnitMockTestRepository)
        {
            _userService = userService;
            _courseResultRepository = courseResultRepository;
            _unitResultRepository = unitResultRepository;
            _placementTestGroupResultRepository = placementTestGroupResultRepository;
            _systemService = systemService;
            _courseUnitMockTestRepository = courseUnitMockTestRepository;
        }

        public async Task<MethodResult<PagingItemsModel<LeaderBoardCollectiveAwardModel>>> Handle(GetLeaderBoardCollectiveQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.EventCode);
            var methodResult = new MethodResult<PagingItemsModel<LeaderBoardCollectiveAwardModel>>();
            List<LeaderBoardCollectiveAwardModel> leaderBoardCollectiveAwards = new List<LeaderBoardCollectiveAwardModel>();

            var studentResult = await _userService.GetStudentByEventCode(request.EventCode);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddError(studentResult.Error);
                return methodResult;
            }

            var students = studentResult.Content?.Result;
            if (students == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(students));
                return methodResult;
            }

            List<Guid> studentIds = students.Select(x => x.Id).ToList();

            var courseResults = await _courseResultRepository.Queryable
                                                             .WhereBulkContains(studentIds, x => x.StudentId)
                                                             .Where(x => x.WorkingStatus == EnumWorkingStatus.Active)
                                                             .ToListAsync(cancellationToken);

            var placementTestGroupResults = await _placementTestGroupResultRepository.Queryable
                                                                                     .WhereBulkContains(studentIds, x => x.StudentId)
                                                                                     .Where(x => x.Status == EnumResultStatus.Done)
                                                                                     .ToListAsync(cancellationToken);

            List<Guid> courseIds = courseResults.Select(x => x.CourseId).Distinct().ToList();
            List<Guid> courseResultIds = courseResults.Select(x => x.Id).Distinct().ToList();
            var unitResults = await _unitResultRepository.Queryable
                                                         .WhereBulkContains(studentIds, x => x.StudentId)
                                                         .WhereBulkContains(courseIds, x => x.CourseId)
                                                         .WhereBulkContains(courseResultIds, x => x.CourseResultId)
                                                         .Where(x => x.Status == EnumResultStatus.Done)
                                                         .ToListAsync(cancellationToken);

            List<Guid> unitIds = unitResults.Select(x => x.UnitId).Distinct().ToList();
            var courseUnitMockTests = await _courseUnitMockTestRepository.Queryable
                                                                         .Where(x => x.UnitId.HasValue && courseIds.Contains(x.CourseId) && unitIds.Contains(x.UnitId.Value))
                                                                         .ToListAsync(cancellationToken);

            var schoolMates = students.Where(x => x.SchoolId.HasValue).GroupBy(x => x.SchoolId!.Value).ToList();

            var schoolResults = await _systemService.GetSchoolsAsync(schoolMates.Select(x => x.Key).ToList());
            if (!schoolResults.IsSuccessStatusCode)
            {
                methodResult.AddError(schoolResults.Error);
                return methodResult;
            }

            var schools = schoolResults.Content?.Result;

            foreach (var schoolMate in schoolMates)
            {
                var unitResultSchoolMates = unitResults.Where(x => schoolMate.Select(c => c.Id).Contains(x.StudentId)).ToList();
                List<Guid> unitSchoolMateIds = unitResultSchoolMates.Select(x => x.UnitId).Distinct().ToList();
                var unitResultSchoolMateGroup = courseUnitMockTests.Where(x => x.UnitId.HasValue && unitSchoolMateIds.Contains(x.UnitId.Value)).ToList();
                var placementTestGroupResultSchoolMates = placementTestGroupResults.Where(x => schoolMate.Select(c => c.Id).Contains(x.StudentId)).Count();

                LeaderBoardCollectiveAwardModel leaderBoardCollectiveAward = new LeaderBoardCollectiveAwardModel
                {
                    SchoolName = schools?.FirstOrDefault(x => x.Id == schoolMate.Key)?.Name,
                    AmountFinishPT = placementTestGroupResultSchoolMates,
                    AmountFinishUnitOne = unitResultSchoolMateGroup.Where(x => x.DisplayOrder == 1).Count(),
                    AmountFinishUnitTwo = unitResultSchoolMateGroup.Where(x => x.DisplayOrder == 2).Count(),
                    AmountFinishUnitThree = unitResultSchoolMateGroup.Where(x => x.DisplayOrder == 3).Count(),
                    AmountFinishUnitFour = unitResultSchoolMateGroup.Where(x => x.DisplayOrder == 4).Count()
                };

                leaderBoardCollectiveAwards.Add(leaderBoardCollectiveAward);
            }

            if (request.SortBy.Any())
            {
                leaderBoardCollectiveAwards = leaderBoardCollectiveAwards.ApplySort(request).ToList();
            }

            int totalItem = leaderBoardCollectiveAwards.Count;
            var lists = leaderBoardCollectiveAwards.ApplyPaging(request).ToList();

            methodResult.Result = new PagingItemsModel<LeaderBoardCollectiveAwardModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
