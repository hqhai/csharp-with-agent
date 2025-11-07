// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.PlacementTestCmd.V1i1
{
    using System.Threading;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Infrastructure.Repositories;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using static Fsel.Shared.Helpers.IeltsScoreHelper;

    public class SavePlacementTestDoneCommand : IRequest<MethodResult<bool>>
    {
        public EnumCourseLevel CourseLevel { get; set; }
        public Guid StudentId { get; set; }
    }

    public class SavePlacementTestDoneCommandHandler : IRequestHandler<SavePlacementTestDoneCommand, MethodResult<bool>>
    {
        private readonly IPlacementTestResultRepository _placementTestResultRepository;
        private readonly IUserService _userService;
        private readonly IPlacementTestRepository _placementTestRepository;
        private readonly IPlacementTestGroupResultRepository _placementTestGroupResultRepository;

        public SavePlacementTestDoneCommandHandler(IPlacementTestResultRepository placementTestResultRepository,
            IUserService userService,
            IPlacementTestRepository placementTestRepository,
            IPlacementTestGroupResultRepository placementTestGroupResultRepository)
        {
            _placementTestResultRepository = placementTestResultRepository;
            _userService = userService;
            _placementTestRepository = placementTestRepository;
            _placementTestGroupResultRepository = placementTestGroupResultRepository;
        }

        public async Task<MethodResult<bool>> Handle(SavePlacementTestDoneCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();
            var studentResult = await _userService.GetUserByStudentId(request.StudentId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }
            var student = studentResult.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }
            if (!student.CourseLevel.HasValue)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student.CourseLevel));
                return methodResult;
            }

            int age = DateTimeHelper.GetYearOld(student.Human?.Birthday);
            var courseLevel = age >= ValueSettings.AgeMilestone.StudentAge ? EnumCourseLevel.B1 : EnumCourseLevel.A2;
            var startingLevel = courseLevel.GetPlacementTestLevelByCourseLevel();

            await SavePlacementTestDoneAsync(student, request.CourseLevel, startingLevel, cancellationToken);
            return methodResult;
        }

        private async Task SavePlacementTestDoneAsync(StudentModel student, EnumCourseLevel desiredLevel, EnumPlacementTestLevel startingLevel, CancellationToken cancellationToken)
        {
            int age = DateTimeHelper.GetYearOld(student.Human?.Birthday);
            var placementTestResultDone = await _placementTestResultRepository.Queryable.Where(x => x.Status == EnumResultStatus.Done && x.StudentId == student.Id)
                                                                          .OrderByDescending(x => x.CreatedDate)
                                                                          .FirstOrDefaultAsync(cancellationToken);
            if (placementTestResultDone != null)
            {
                var (levelNext, isLock) = placementTestResultDone.Level.GetLevelInScore(placementTestResultDone.Percent, GetInitialAge(startingLevel, age));
                if (isLock)
                {
                    await UpdatePlacementGroupResultDoneAsync(placementTestResultDone, desiredLevel, levelNext);
                    await _userService.UpdateStudentByLevelAsync(new UpdateStudentByLevelModel { Id = student.Human?.UserId ?? default, CourseLevel = levelNext ?? default, BaseCourseLevel = levelNext ?? default });
                    return;
                }
                else if (levelNext.HasValue)
                {
                    await AddPlacementTestResultAndGetPlacementTest(desiredLevel, startingLevel, levelNext.Value.GetPlacementTestLevelByCourseLevel(), student.Id, cancellationToken);
                }
            }
            else
            {
                await AddPlacementTestResultAndGetPlacementTest(desiredLevel, startingLevel, startingLevel, student.Id, cancellationToken);
            }
            await SavePlacementTestDoneAsync(student, desiredLevel, startingLevel, cancellationToken);
        }

        private async Task CreatePlacementGroupResultAsync(PlacementTest placementTest, Guid studentId)
        {
            var placementTestGroupResult = await _placementTestGroupResultRepository.Queryable.FirstOrDefaultAsync(x => x.StudentId == studentId);
            if (placementTestGroupResult != null)
            {
                return;
            }
            placementTestGroupResult = new PlacementTestGroupResult
            {
                StudentId = studentId,
                NewDate = DateTime.UtcNow,
                ProcessDate = DateTime.UtcNow,
                ProcessLevel = placementTest.Level,
                Status = EnumResultStatus.Process
            };
            await _placementTestGroupResultRepository.BulkMergeAsync(new List<PlacementTestGroupResult> { placementTestGroupResult }, bulk =>
            {
                bulk.ColumnPrimaryKeyExpression = c => new { c.StudentId, c.IsDeleted };
            });
        }

        private async Task UpdatePlacementGroupResultDoneAsync(PlacementTestResult placementTestResult, EnumCourseLevel desiredLevel, EnumCourseLevel? level)
        {
            var placementTestGroupResult = await _placementTestGroupResultRepository.Queryable.FirstOrDefaultAsync(x => x.StudentId == placementTestResult.StudentId);
            if (placementTestGroupResult == null)
            {
                return;
            }

            placementTestGroupResult.CompletionDate = DateTime.UtcNow;
            placementTestGroupResult.CompletionLevel = placementTestResult.Level;
            placementTestGroupResult.SuggetLevel = level;
            placementTestGroupResult.ChooseLevel = desiredLevel;
            placementTestGroupResult.CurrentLevel = SendMailHelper.GetPreviousEnumValue(level ?? default);
            placementTestGroupResult.Status = EnumResultStatus.Done;
            placementTestGroupResult.Percent = placementTestResult.Percent;
            await _placementTestGroupResultRepository.BulkUpdateList(new List<PlacementTestGroupResult> { placementTestGroupResult }, bulk =>
            {
                bulk.IgnoreOnUpdateExpression = c => new { c.StudentId };
            });
        }

        private async Task AddPlacementTestResultAndGetPlacementTest(EnumCourseLevel desiredLevel, EnumPlacementTestLevel startingLevel, EnumPlacementTestLevel level, Guid studentId, CancellationToken cancellationToken)
        {
            Random random = new Random();
            var placementTestResult = await _placementTestResultRepository.Queryable.Where(x => x.StudentId == studentId && x.Level == level).FirstOrDefaultAsync(cancellationToken);
            var placementTestQuery = _placementTestRepository.Queryable;
            if (placementTestResult != null)
            {
                return;
            }
            var placementTests = await placementTestQuery.Where(x => x.Level == level && x.IsActive).ToListAsync(cancellationToken);
            var placementTest = placementTests.OrderBy(x => random.Next()).FirstOrDefault();
            if (placementTest == null)
            {
                return;
            }
            await CreatePlacementGroupResultAsync(placementTest, studentId);
            var correctValue = PlacementTestHelper.GetCorrectCountToLevel(desiredLevel, startingLevel.GetCourseLevelByPlacementTestLevel(), level);
            placementTestResult = new PlacementTestResult
            {
                Level = placementTest.Level,
                PlacementTestId = placementTest.Id,
                StudentId = studentId,
                Status = EnumResultStatus.Done,
                CorrectCount = correctValue.Item1,
                CorrectTotal = correctValue.Item2,
            };

            await _placementTestResultRepository.BulkMergeAsync(new List<PlacementTestResult> { placementTestResult }, bulk =>
            {
                bulk.ColumnPrimaryKeyExpression = c => new { c.StudentId, c.PlacementTestId, c.IsDeleted };
            });
        }
    }
}
