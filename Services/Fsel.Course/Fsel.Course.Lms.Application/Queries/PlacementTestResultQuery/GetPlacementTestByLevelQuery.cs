// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.Helpers;

namespace Fsel.Course.Lms.Application.Queries.PlacementTestResultQuery
{
    using System;
    using System.Linq.Dynamic.Core;
    using System.Threading;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    public class GetPlacementTestByLevelQuery : IRequest<MethodResult<PlacementTestDtoModel>>
    {
    }

    public class GetPlacementTestByLevelQueryHandler : IRequestHandler<GetPlacementTestByLevelQuery, MethodResult<PlacementTestDtoModel>>
    {
        private readonly AuthContext _authContext;
        private readonly SectionGroupConverter _sectionGroupConverter;
        private readonly IPlacementTestResultRepository _placementTestResultRepository;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly ILogger<GetPlacementTestByLevelQuery> _logger;
        private readonly IPlacementTestRepository _placementTestRepository;
        private readonly IPlacementTestGroupResultRepository _placementTestGroupResultRepository;

        public GetPlacementTestByLevelQueryHandler(AuthContext authContext
            , SectionGroupConverter sectionGroupConverter
            , IPlacementTestResultRepository placementTestResultRepository
            , IUserService userService
            , IMapper mapper
            , ILogger<GetPlacementTestByLevelQuery> logger
            , IPlacementTestRepository placementTestRepository
            , IPlacementTestGroupResultRepository placementTestGroupResultRepository)
        {
            _authContext = authContext;
            _sectionGroupConverter = sectionGroupConverter;
            _placementTestResultRepository = placementTestResultRepository;
            _userService = userService;
            _mapper = mapper;
            _logger = logger;
            _placementTestRepository = placementTestRepository;
            _placementTestGroupResultRepository = placementTestGroupResultRepository;
        }

        public async Task<MethodResult<PlacementTestDtoModel>> Handle(GetPlacementTestByLevelQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PlacementTestDtoModel> methodResult = new MethodResult<PlacementTestDtoModel>();
            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }
            var student = studentResult?.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }
            if (student.Human == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student.Human));
                return methodResult;
            }

            int age = DateTimeHelper.GetYearOld(student.Human.Birthday);
            if (!student.CourseLevel.HasValue)
            {
                student.CourseLevel = age >= ValueSettings.AgeMilestone.StudentAge ? EnumCourseLevel.B1 : EnumCourseLevel.A2;
            }
            var isPlacementTest = await _placementTestResultRepository.CheckByPassPlacementTestAsync(student.Id, age);
            if (isPlacementTest.Item2)
            {
                methodResult.AddErrorBadRequest(nameof(EnumPlacementTestErrorCode.PlacementTestLock));
                return methodResult;
            }
            var (placementTest, placementTestResult) = await AddPlacementTestResultAndGetPlacementTest(student.CourseLevel.Value.GetPlacementTestLevelByCourseLevel(), student.Id, cancellationToken);
            if (placementTest == null || placementTestResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = await GetPlacmentTestAsync(placementTest, placementTestResult);
            if (placementTestResult.Status == EnumResultStatus.Done)
            {
                _logger.LogInformation($"Logger PT Done : {methodResult.Result.Serialize()}");
            }
            return methodResult;
        }

        private async Task<PlacementTestDtoModel> GetPlacmentTestAsync(PlacementTest placementTest, PlacementTestResult placementTestResult)
        {
            ArgumentNullException.ThrowIfNull(placementTest);
            var placementTestDto = _mapper.Map<PlacementTestDtoModel>(placementTest);
            var sectionGroups = placementTest.PlacementTestSections.OrderBy(x => x.CreatedDate).Select(x => x.SectionGroup ?? new SectionGroup()).ToList();
            placementTestDto.TotalQuestion = _sectionGroupConverter.GetTotalQuestion(sectionGroups);
            placementTestDto.PlacementTestResult = _mapper.Map<PlacementTestResultModel>(placementTestResult);
            placementTestDto.SectionGroups = await _sectionGroupConverter.GetSectionGroupsAsync(sectionGroups, placementTestDto.PlacementTestResult.Id, nameof(SectionGroupResult.PlacementTestResultId));
            return placementTestDto;
        }

        private async Task<(PlacementTest?, PlacementTestResult?)> AddPlacementTestResultAndGetPlacementTest(EnumPlacementTestLevel level, Guid studentId, CancellationToken cancellationToken)
        {
            Random random = new Random();
            var placementTest = new PlacementTest();
            var placementTestResult = await _placementTestResultRepository.Queryable.Where(x => x.StudentId == studentId && x.Level == level).FirstOrDefaultAsync(cancellationToken);
            var placementTestQuery = _placementTestRepository.Queryable.Include(x => x.PlacementTestSections)
                                                            .ThenInclude(x => x.SectionGroup)
                                                            .ThenInclude(x => x!.Sections)
                                                            .ThenInclude(x => x.SectionQuestions)
                                                           .Include(x => x.PlacementTestSections)
                                                           .ThenInclude(x => x.SectionGroup)
                                                           .ThenInclude(x => x!.SectionGroupResults.Where(x => x.StudentId == studentId));
            if (placementTestResult == null)
            {
                var placementTests = await placementTestQuery.Where(x => x.Level == level && x.IsActive).ToListAsync(cancellationToken);
                placementTest = placementTests.OrderBy(x => random.Next()).FirstOrDefault();
                if (placementTest != null)
                {
                    var placementTestGroupResult = await SavePlacementGroupResultAsync(placementTest, studentId);
                    placementTestResult = new PlacementTestResult
                    {
                        Level = placementTest.Level,
                        PlacementTestId = placementTest.Id,
                        StudentId = studentId,
                        Status = EnumResultStatus.New,
                        PlacementTestGroupResultId = placementTestGroupResult.Id
                    };

                    try
                    {
                        await _placementTestResultRepository.BulkMergeAsync(new List<PlacementTestResult> { placementTestResult }, bulk =>
                        {
                            bulk.ColumnPrimaryKeyExpression = c => new { c.PlacementTestId, c.StudentId, c.IsDeleted };
                        });
                    }
                    catch
                    {
                        placementTestResult = await _placementTestResultRepository.Queryable.Where(x => x.StudentId == studentId && x.Level == level).FirstOrDefaultAsync(cancellationToken);
                    }
                }
            }
            else
            {
                placementTest = await placementTestQuery.Where(x => x.Id == placementTestResult.PlacementTestId).FirstOrDefaultAsync(cancellationToken);
            }
            return (placementTest, placementTestResult);
        }

        private async Task<PlacementTestGroupResult> SavePlacementGroupResultAsync(PlacementTest placementTest, Guid studentId)
        {
            var placementTestGroupResult = await _placementTestGroupResultRepository.Queryable.FirstOrDefaultAsync(x => x.StudentId == studentId);
            if (placementTestGroupResult != null)
            {
                return placementTestGroupResult;
            }
            placementTestGroupResult = new PlacementTestGroupResult
            {
                StudentId = studentId,
                ProcessLevel = placementTest.Level,
                NewDate = DateTime.UtcNow,
                Status = EnumResultStatus.New,
            };
            await _placementTestGroupResultRepository.BulkMergeAsync(new List<PlacementTestGroupResult> { placementTestGroupResult }, bulk =>
            {
                bulk.ColumnPrimaryKeyExpression = c => new { c.StudentId, c.IsDeleted };
            });
            return placementTestGroupResult;
        }
    }
}
