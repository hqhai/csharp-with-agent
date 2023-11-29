// Copyright (c) Atlantic. All rights reserved.

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
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetPlacementTestByLevelQuery : IRequest<MethodResult<PlacementTestDtoModel>>
    {
    }

    public class GetPlacementTestByLevelQueryHandler : IRequestHandler<GetPlacementTestByLevelQuery, MethodResult<PlacementTestDtoModel>>
    {
        private readonly AuthContext _authContext;
        private readonly SectionConverter _sectionConverter;
        private readonly IPlacementTestResultRepository _placementTestResultRepository;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly IPlacementTestRepository _placementTestRepository;

        public GetPlacementTestByLevelQueryHandler(AuthContext authContext
            , SectionConverter sectionConverter
            , IPlacementTestResultRepository placementTestResultRepository
            , IUserService userService
            , IMapper mapper
            , IPlacementTestRepository placementTestRepository)
        {
            _authContext = authContext;
            _sectionConverter = sectionConverter;
            _placementTestResultRepository = placementTestResultRepository;
            _userService = userService;
            _mapper = mapper;
            _placementTestRepository = placementTestRepository;
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
            var studentId = student?.Id ?? default;
            int age = DateTimeHelper.GetYearOld(student?.Human?.Birthday);
            var placementTestResultDone = await _placementTestResultRepository.Queryable.Where(x => x.Status == EnumResultStatus.Done && x.StudentId == studentId)
                                                                          .OrderByDescending(x => x.CreatedDate)
                                                                          .FirstOrDefaultAsync(cancellationToken);
            if (placementTestResultDone != null)
            {
                var (levelNext, isLock) = placementTestResultDone.Level.GetLevelInScore(placementTestResultDone.Percent, age);
                if (isLock)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumPlacementTestErrorCode.PlacementTestLock), nameof(levelNext));
                    return methodResult;
                }
            }
            var (placementTest, placementTestResult) = await AddPlacementTestResultAndGetPlacementTest(student!.CourseLevel.GetPlacementTestLevelByCourseLevel(), studentId, cancellationToken);
            if (placementTest == null || placementTestResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }
            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = GetPlacmentTestAsync(placementTest, placementTestResult);
            return methodResult;
        }

        private PlacementTestDtoModel GetPlacmentTestAsync(PlacementTest placementTest, PlacementTestResult placementTestResult)
        {
            ArgumentNullException.ThrowIfNull(placementTest);
            var placementTestDto = _mapper.Map<PlacementTestDtoModel>(placementTest);
            var sectionGroups = placementTest.PlacementTestSections.OrderBy(x => x.CreatedDate).Select(x => x.SectionGroup ?? new SectionGroup()).ToList();
            placementTestDto.TotalQuestion = _sectionConverter.GetTotalQuestion(sectionGroups);
            placementTestDto.PlacementTestResult = _mapper.Map<PlacementTestResultModel>(placementTestResult);
            placementTestDto.CourseSkills = _sectionConverter.GetCourseSkill(sectionGroups);
            placementTestDto.SectionGroups = _sectionConverter.GetSectionGroups(sectionGroups, placementTestDto.PlacementTestResult.Id, "PlacementTestResultId");
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
                    placementTestResult = new PlacementTestResult { Level = placementTest.Level, PlacementTestId = placementTest.Id, StudentId = studentId };
                    _placementTestResultRepository.Add(placementTestResult);
                    await _placementTestResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }
            }
            else
            {
                placementTest = await placementTestQuery.Where(x => x.Id == placementTestResult.PlacementTestId).FirstOrDefaultAsync(cancellationToken);
            }
            return (placementTest, placementTestResult);
        }
    }
}
