// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.ChangeCourse
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.Enums;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels.ChangeCourseModels;
    using Fsel.Course.Lms.Application.Commands.CourseResultCmd.V1i2;
    using Fsel.Course.Lms.Application.Services.ApplicationServices;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.CacheServices;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.CommandModels;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public interface IChangeCourseService
    {
        Task<TestGroupResult> InitForMustDoPtProgram(ChangeCourseModel changeCourseRequest);

        Task<TestGroupResult> InitForProgramByPassPt(ChangeCourseModel changeCourseRequest);

        Task<TestGroupResult> InitForProgramExistedPt(Guid relatedHistoryId, Guid toProgramId, Guid relatedPtResultId);

        Task SelectCourseLevelAfterPt(SelectCourseLevelRequest request);

        Task SwitchDirectlyToNewCourse(SelectCourseLevelRequest request);

        Task SwitchDirectlyToNewCourseForChangeCourse(SelectCourseLevelRequest request);

        Task SwitchDirectlyToExistCourseForChangeLevel(Guid courseResultId, Guid studentId);

        Task SwitchDirectlyToExistCourseForSelectLevelAfterPt(Guid courseResultId, Guid studentId, Guid? relatedHistoryId = null);

        Task<ChangeCourseAggregate> GetChangeCourseAggreate(StudentModel student, Guid? levelId, CancellationToken cancellationToken);

        Task<ChangeCourseAggregate> GetChangeProgramAggreate(StudentModel student, Guid programId, CancellationToken cancellationToken);
    }

    public class ChangeCourseService : IChangeCourseService
    {
        private readonly ICourseChangingHistoryRepository _courseChangingHistoryRepository;
        private readonly ITestGroupResultRepository _testGroupResultRepository;
        private readonly IFlowService _flowService;
        private readonly ICourseRepository _courseRepository;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly ICategoryCachingService _categoryCachingService;
        private readonly IUserService _userService;
        private readonly ISubjectConditionRepository _categoryRepository;
        private readonly IMediator _mediator;

        public ChangeCourseService(ICourseChangingHistoryRepository courseChangingHistoryRepository,
            ITestGroupResultRepository testGroupResultRepository,
            IFlowService flowService,
            ICourseRepository courseRepository,
            ICourseResultRepository courseResultRepository,
            IUserService userService,
            ICategoryCachingService categoryCachingService,
            ISubjectConditionRepository categoryRepository,
            IMediator mediator)
        {
            _courseChangingHistoryRepository = courseChangingHistoryRepository;
            _testGroupResultRepository = testGroupResultRepository;
            _flowService = flowService;
            _courseRepository = courseRepository;
            _courseResultRepository = courseResultRepository;
            _categoryCachingService = categoryCachingService;
            _userService = userService;
            _categoryRepository = categoryRepository;
            _mediator = mediator;
        }

        public async Task<TestGroupResult> InitForMustDoPtProgram(ChangeCourseModel changeCourseRequest)
        {
            ArgumentNullException.ThrowIfNull(changeCourseRequest);

            var flowMatch = await _flowService.GetHierarchicalFlowByCondition(x => x.ProgramId == changeCourseRequest.OwnPtProgramId
                                                                                   && x.Status == EnumStatus.Active
                                                                                   && x.FromAge <= changeCourseRequest.Age && x.ToAge >= changeCourseRequest.Age);
            if (flowMatch?.StepFlows.FirstOrDefault() == null)
            {
                return null;
            }

            var firstStepFlow = flowMatch.StepFlows?.FirstOrDefault();
            if (firstStepFlow == null)
            {
                return null;
            }

            var testGroupResult = new TestGroupResult
            {
                Id = Guid.NewGuid(),
                ProgramId = changeCourseRequest.ToProgramId,
                ProgramIdOfPt = changeCourseRequest.OwnPtProgramId,
                FlowId = flowMatch.Id,
                StudentId = changeCourseRequest.StudentId,
                TestType = EnumTestType.PlacementTest,
                Status = EnumResultStatus.New
            };
            _testGroupResultRepository.Add(testGroupResult);

            var waitSelectProgramHistory = await _courseChangingHistoryRepository.Queryable
                .Where(x => x.StudentId == changeCourseRequest.StudentId && x.Status == EnumChangingStatus.InprogressSelectProgram)
                .OrderByDescending(x => x.CreatedDate)
                .FirstOrDefaultAsync();

            if (waitSelectProgramHistory == null)
            {
                var history = new CourseChangingHistory
                {
                    Id = Guid.NewGuid(),
                    StudentId = changeCourseRequest.StudentId,
                    ToLevelId = changeCourseRequest.ToLevelId,
                    ToProgramId = changeCourseRequest.ToProgramId,
                    PtResultId = testGroupResult.Id,
                    FromInfo = changeCourseRequest.FromInfo,
                    Action = changeCourseRequest.Action,
                    CreatedDate = DateTime.UtcNow,
                    Status = EnumChangingStatus.InProgressPt
                };
                _courseChangingHistoryRepository.Add(history);
            }
            else
            {
                waitSelectProgramHistory.ToLevelId = changeCourseRequest.ToLevelId;
                waitSelectProgramHistory.ToProgramId = changeCourseRequest.ToProgramId;
                waitSelectProgramHistory.Status = EnumChangingStatus.InProgressPt;
            }

            var courseResults = await _courseResultRepository.Queryable
                .Where(x => x.StudentId == changeCourseRequest.StudentId && x.WorkingStatus == EnumWorkingStatus.Active)
                .ToListAsync();
            foreach (var courseResult in courseResults)
            {
                courseResult.WorkingStatus = EnumWorkingStatus.InActive;
            }

            await _testGroupResultRepository.UnitOfWork.SaveChangesAsync();

            await _userService.UpdateLearningContextAsync(new UpdateStudentLearningContextCommandModel());

            return testGroupResult;
        }

        public async Task<TestGroupResult> InitForProgramExistedPt(Guid relatedHistoryId, Guid toProgramId, Guid relatedPtResultId)
        {
            var waitSelectProgramHistory = await _courseChangingHistoryRepository.Queryable
               .Where(x => x.Id == relatedHistoryId && x.Status == EnumChangingStatus.InprogressSelectProgram)
               .OrderByDescending(x => x.CreatedDate)
               .FirstOrDefaultAsync();

            if (waitSelectProgramHistory != null)
            {
                waitSelectProgramHistory.ToProgramId = toProgramId;
                waitSelectProgramHistory.PtResultId = relatedPtResultId;
                waitSelectProgramHistory.Status = EnumChangingStatus.InProgressSelectCourse;
            }
            await _testGroupResultRepository.UnitOfWork.SaveChangesAsync();

            var testGroupResult = await _testGroupResultRepository.Queryable
                .Where(x => x.Id == relatedPtResultId)
                .FirstOrDefaultAsync();

            return testGroupResult;
        }

        public async Task<TestGroupResult> InitForProgramByPassPt(ChangeCourseModel changeCourseRequest)
        {
            ArgumentNullException.ThrowIfNull(changeCourseRequest);

            var testGroupResult = new TestGroupResult
            {
                Id = Guid.NewGuid(),
                ProgramId = changeCourseRequest.ToProgramId,
                ProgramIdOfPt = changeCourseRequest.OwnPtProgramId,
                StudentId = changeCourseRequest.StudentId,
                TestType = EnumTestType.PlacementTest,
                Status = EnumResultStatus.ByPass
            };
            _testGroupResultRepository.Add(testGroupResult);

            var history = new CourseChangingHistory
            {
                Id = Guid.NewGuid(),
                StudentId = changeCourseRequest.StudentId,
                ToLevelId = changeCourseRequest.ToLevelId,
                ToProgramId = changeCourseRequest.ToProgramId,
                PtResultId = testGroupResult.Id,
                FromInfo = changeCourseRequest.FromInfo,
                Action = changeCourseRequest.Action,
                CreatedDate = DateTime.UtcNow,
                Status = EnumChangingStatus.InProgressSelectCourse
            };

            _courseChangingHistoryRepository.Add(history);
            await _testGroupResultRepository.UnitOfWork.SaveChangesAsync();

            await _userService.UpdateLearningContextAsync(new UpdateStudentLearningContextCommandModel());

            return testGroupResult;
        }

        public async Task SelectCourseLevelAfterPt(SelectCourseLevelRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var courses = await _courseRepository.ReadQueryable.Include(x => x.Program)
                .ThenInclude(x => x.CategoryParent)
                .Where(x => x.ProgramId == request.SelectedProgramId
                            && x.LevelId == request.SelectedLevelId
                            && x.VersionStatus == EnumVersionStatus.LastVersion
                            && !x.IsArchive && x.Status == EnumCourseStatus.Active)
                .ToListAsync();
            var random = new Random();
            var course = courses.OrderBy(x => random.Next()).FirstOrDefault();

            if (course == null)
            {
                return;
            }

            var createCourseResult = await _mediator.Send(new SaveCourseResultCommand
            {
                StudentId = request.StudentId,
                CourseId = course.Id
            });

            if (createCourseResult?.Result != null && createCourseResult.IsOK)
            {
                var activeCourseResults = await _courseResultRepository.Queryable
                .Where(x => x.StudentId == request.StudentId && x.WorkingStatus == EnumWorkingStatus.Active)
                .ToListAsync();
                activeCourseResults.ForEach(cr =>
                {
                    if (cr.Id != createCourseResult.Result?.Id)
                    {
                        cr.WorkingStatus = EnumWorkingStatus.InActive;
                    }
                });

                if (request.RelatedHistoryId != null)
                {
                    var courseChangingHistory = await _courseChangingHistoryRepository.Queryable
                                                    .Where(x => x.Id == request.RelatedHistoryId.Value)
                                                    .FirstOrDefaultAsync();

                    if (courseChangingHistory?.Status == EnumChangingStatus.InProgressSelectCourse)
                    {
                        courseChangingHistory.SelectedLevelId = request.SelectedLevelId;
                        courseChangingHistory.SelectedProgramId = request.SelectedProgramId;
                        courseChangingHistory.Status = EnumChangingStatus.Completed;
                        courseChangingHistory.ToCourseResultId = createCourseResult.Result.Id;
                    }
                }
                await _testGroupResultRepository.UnitOfWork.SaveChangesAsync();
                await UpdateLearningContextAsync(course);
            }
        }

        public async Task SwitchDirectlyToNewCourse(SelectCourseLevelRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var courses = await _courseRepository.ReadQueryable
                .Include(x => x.Program)
                .ThenInclude(x => x.CategoryParent)
                .Where(x => x.ProgramId == request.SelectedProgramId
                            && x.LevelId == request.SelectedLevelId
                            && x.VersionStatus == EnumVersionStatus.LastVersion
                            && !x.IsArchive && x.Status == EnumCourseStatus.Active)
                .ToListAsync();
            var random = new Random();
            var course = courses.OrderBy(x => random.Next()).FirstOrDefault();

            if (course == null)
            {
                return;
            }

            var createCourseResult = await _mediator.Send(new SaveCourseResultCommand
            {
                StudentId = request.StudentId,
                CourseId = course.Id
            });

            if (createCourseResult?.Result != null && createCourseResult.IsOK)
            {
                var activeCourseResults = await _courseResultRepository.Queryable
                .Where(x => x.StudentId == request.StudentId && x.WorkingStatus == EnumWorkingStatus.Active)
                .ToListAsync();

                activeCourseResults.ForEach(cr =>
                {
                    if (cr.Id != createCourseResult.Result?.Id)
                    {
                        cr.WorkingStatus = EnumWorkingStatus.InActive;
                    }
                });

                if (request.RelatedHistoryId != null)
                {
                    var courseChangingHistory = await _courseChangingHistoryRepository.Queryable
                        .FirstOrDefaultAsync(x => x.Id == request.RelatedHistoryId.Value);
                    if (courseChangingHistory != null && courseChangingHistory.Status == EnumChangingStatus.InProgressSelectCourse)
                    {
                        courseChangingHistory.Status = EnumChangingStatus.Completed;
                        courseChangingHistory.ToCourseResultId = createCourseResult.Result.Id;
                        await _testGroupResultRepository.UnitOfWork.SaveChangesAsync();
                    }
                }
                await UpdateLearningContextAsync(course);
            }
        }

        public async Task SwitchDirectlyToNewCourseForChangeCourse(SelectCourseLevelRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var courses = await _courseRepository.ReadQueryable
                .Include(x => x.Program)
                .ThenInclude(x => x.CategoryParent)
                .Where(x => x.ProgramId == request.SelectedProgramId
                            && x.LevelId == request.SelectedLevelId
                            && x.VersionStatus == EnumVersionStatus.LastVersion
                            && !x.IsArchive && x.Status == EnumCourseStatus.Active)
                .ToListAsync();
            var random = new Random();
            var course = courses.OrderBy(x => random.Next()).FirstOrDefault();

            if (course == null)
            {
                return;
            }

            var createCourseResult = await _mediator.Send(new SaveCourseResultCommand
            {
                StudentId = request.StudentId,
                CourseId = course.Id
            });

            if (createCourseResult?.Result != null && createCourseResult.IsOK)
            {
                var activeCourseResults = await _courseResultRepository.Queryable
                .Where(x => x.StudentId == request.StudentId && x.WorkingStatus == EnumWorkingStatus.Active)
                .ToListAsync();

                activeCourseResults.ForEach(cr =>
                {
                    if (cr.Id != createCourseResult.Result?.Id)
                    {
                        cr.WorkingStatus = EnumWorkingStatus.InActive;
                    }
                });

                var history = new CourseChangingHistory
                {
                    Id = Guid.NewGuid(),
                    StudentId = request.StudentId,
                    ToLevelId = request.ToLevelId,
                    ToProgramId = request.ToProgramId,
                    SelectedLevelId = request.SelectedLevelId,
                    SelectedProgramId = request.SelectedProgramId,
                    FromInfo = request.FromInfo,
                    Action = request.Action,
                    CreatedDate = DateTime.UtcNow,
                    Status = EnumChangingStatus.Completed,
                    PtResultId = request.PtResultId,
                    ToCourseResultId = createCourseResult.Result.Id
                };

                _courseChangingHistoryRepository.Add(history);
                await _testGroupResultRepository.UnitOfWork.SaveChangesAsync();
                await UpdateLearningContextAsync(course);
            }
        }

        public async Task SwitchDirectlyToExistCourseForSelectLevelAfterPt(Guid courseResultId, Guid studentId, Guid? relatedHistoryId = null)
        {
            var courseResults = _courseResultRepository.Queryable
                .Where(x => x.StudentId == studentId && x.WorkingStatus == EnumWorkingStatus.Active)
                .ToList();

            courseResults.ForEach(cr =>
            {
                cr.WorkingStatus = EnumWorkingStatus.InActive;
            });

            var targetCourseResult = await _courseResultRepository.Queryable
                .Include(x => x.Course)
                .ThenInclude(x => x.Program)
                .ThenInclude(x => x.CategoryParent)
                .FirstOrDefaultAsync(x => x.Id == courseResultId);

            if (targetCourseResult == null)
            {
                throw new Exception("Target course result not found");
            }

            targetCourseResult.WorkingStatus = EnumWorkingStatus.Active;

            if (relatedHistoryId != null)
            {
                var courseChangingHistory = await _courseChangingHistoryRepository.Queryable
                    .FirstOrDefaultAsync(x => x.Id == relatedHistoryId.Value);
                if (courseChangingHistory != null && courseChangingHistory.Status == EnumChangingStatus.InProgressSelectCourse)
                {
                    courseChangingHistory.ToCourseResultId = targetCourseResult.Id;
                    courseChangingHistory.Status = EnumChangingStatus.Completed;
                }
            }

            await _testGroupResultRepository.UnitOfWork.SaveChangesAsync();
            await UpdateLearningContextAsync(targetCourseResult.Course);
        }

        public async Task SwitchDirectlyToExistCourseForChangeLevel(Guid courseResultId, Guid studentId)
        {
            var courseResults = _courseResultRepository.Queryable.Include(x => x.Course)
                .ThenInclude(x => x.Program)
                .ThenInclude(x => x.CategoryParent)
                .Where(x => x.StudentId == studentId && x.WorkingStatus == EnumWorkingStatus.Active)
                .ToList();

            courseResults.ForEach(cr =>
            {
                cr.WorkingStatus = EnumWorkingStatus.InActive;
            });

            var targetCourseResult = await _courseResultRepository.Queryable
                .FirstOrDefaultAsync(x => x.Id == courseResultId);

            if (targetCourseResult == null)
            {
                throw new Exception("Target course result not found");
            }

            targetCourseResult.WorkingStatus = EnumWorkingStatus.Active;

            await _testGroupResultRepository.UnitOfWork.SaveChangesAsync();
            await UpdateLearningContextAsync(targetCourseResult.Course);
        }

        private async Task UpdateLearningContextAsync(Course? course)
        {
            await _userService.UpdateLearningContextAsync(new UpdateStudentLearningContextCommandModel
            {
                CourseId = course?.Id,
                LevelId = course?.LevelId,
                ProgramId = course?.ProgramId,
                SubjectId = course?.Program?.CategoryParent?.Id
            });
        }

        public async Task<ChangeCourseAggregate> GetChangeCourseAggreate(StudentModel student, Guid? levelId, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(student?.Id);
            ArgumentNullException.ThrowIfNull(levelId);

            //var userCourseSettingsResult = await _userService.GetUserCourseSettingsAsync(student.UserId);
            //if (!userCourseSettingsResult.IsSuccessStatusCode)
            //{
            //    return null;
            //}

            //var userCourseSettings = userCourseSettingsResult.Content?.Result;
            //if (!userCourseSettings.HasRemainingAttempts(EnumUserCourseType.ChangeLevel))
            //{
            //    return null;
            //}

            //var currentCourse = await _courseResultRepository.Queryable
            //    .Include(x => x.Course)
            //    .Where(x => x.StudentId == student.Id && x.CourseId == student.CourseId && x.WorkingStatus == EnumWorkingStatus.Active)
            //    .OrderByDescending(x => x.CreatedDate)
            //    .FirstOrDefaultAsync(cancellationToken);

            var subjects = await _categoryCachingService.GetAll(cancellationToken);

            var testGroupResults = await _testGroupResultRepository.ReadQueryable
                .Include(x => x.CourseChangingHistories)
                .Include(x => x.CurrentLevel)
                .Where(x => x.StudentId == student.Id && x.Status == EnumResultStatus.Done)
                .ToListAsync(cancellationToken);

            var courseResults = await _courseResultRepository.ReadQueryable
                .Include(x => x.Course)
                .Where(x => x.StudentId == student.Id)
                .ToListAsync(cancellationToken);

            var currentCourse = courseResults
                .Where(x => x.StudentId == student.Id && x.CourseId == student.CourseId && x.WorkingStatus == EnumWorkingStatus.Active)
                .OrderByDescending(x => x.CreatedDate)
                .FirstOrDefault();

            var courseChangingHistories = _courseChangingHistoryRepository.ReadQueryable
                .Where(x => x.StudentId == student.Id && (x.Status == EnumChangingStatus.Completed || x.Status == EnumChangingStatus.InProgressSelectCourse))
                .ToList();

            var changeCourseAggregateBuilder = new ChangeCourseAggregateBuilder(subjects, testGroupResults, courseChangingHistories, currentCourse, student.User, courseResults)
                .BuildTree(levelId);

            changeCourseAggregateBuilder.BuildProgramInfo();
            changeCourseAggregateBuilder.BuildLevelInfomation();

            await changeCourseAggregateBuilder.BuildLevelAccess(async projectId =>
            {
                var subjectCondition = await _categoryRepository.ReadQueryable
                    .Include(x => x.SubjectConditionRules)
                    .FirstOrDefaultAsync(x => x.CategoryId == projectId && x.Status && x.Type == EnumConditionType.CourseSuggest, cancellationToken: cancellationToken);

                return subjectCondition?.SubjectConditionRules ?? new List<SubjectConditionRule>();
            });

            return changeCourseAggregateBuilder.Build();
        }

        public async Task<ChangeCourseAggregate> GetChangeProgramAggreate(StudentModel student, Guid programId, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(student?.Id);

            var subjects = await _categoryCachingService.GetAll(cancellationToken);
            var testGroupResults = await _testGroupResultRepository.ReadQueryable
                .Include(x => x.CurrentLevel)
                .Where(x => x.StudentId == student.Id && x.Status == EnumResultStatus.Done)
                .ToListAsync(cancellationToken);
            var changeProgramRequest = new ChangeProgramRequest { ProgramId = programId };
            var changeCourseAggregateBuilder =
                new ChangeCourseAggregateBuilder(subjects, testGroupResults, null, null, student.User)
                    .BuildTree(null)
                    .BuildProgramInfo();

            return changeCourseAggregateBuilder.Build();
        }
    }

    public record ChangeCourseModel
    {
        public Guid StudentId { get; set; }
        public int Age { get; set; }
        public Guid ToProgramId { get; set; }
        public Guid ToLevelId { get; set; }
        public Guid OwnPtProgramId { get; set; }
        public FromInfo? FromInfo { get; set; }
        public EnumChangeCourseAction Action { get; set; }
    }

    public record SelectCourseLevelRequest
    {
        public Guid StudentId { get; set; }
        public Guid ToProgramId { get; set; }
        public Guid? ToLevelId { get; set; }
        public Guid SelectedProgramId { get; set; }
        public Guid? SelectedLevelId { get; set; }
        public Guid? PtResultId { get; set; }
        public FromInfo? FromInfo { get; set; }
        public Guid? RelatedHistoryId { get; set; }
        public EnumChangeCourseAction Action { get; set; }
    }
}
