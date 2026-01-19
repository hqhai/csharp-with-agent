// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Test
{
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Models.EntityModels.UserNavigationActionModels;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;

    public class TestNavigation
    {
        // user are learning and no history ( this case only happen when user just finish placement test and start learning in old system)
        [Fact]
        public void TestCase1()
        {
            var course = new Course
            {
                LevelId = Data.a1.Id,
                Level = new Level
                {
                    Id = Data.a1.Id,
                    ProgramId = Data.englishSubject.Categorys.ElementAt(0).Categorys.First(x => x.Name == "Academic English").Id
                }
            };

            // Arrange
            var student = new StudentModel { Id = Guid.NewGuid(), User = new UserModel { Birthday = new DateTime(2014, 1, 1), }, CourseId = course.Id };

            var ptResults = new List<TestGroupResult>
            {
                new TestGroupResult
                {
                    Id = Guid.NewGuid(),
                    StudentId = student.Id,
                    TestType = EnumTestType.PlacementTest,
                    CurrentLevelId = Data.a2.Id,
                    Percent = 85,
                    Status = EnumResultStatus.Done
                }
            };

            var currentCourseResult = new CourseResult
            {
                Id = Guid.NewGuid(),
                StudentId = student.Id,
                CourseId = student.CourseId.Value,
                TestGroupResults = ptResults,
                Course = course
            };

            var currentStateInfo = new CurrentStateInfo
            {
                LevelId = currentCourseResult.Course.LevelId,
                CourseResultId = currentCourseResult.Id,
                ProgramId = currentCourseResult.Course.Level.ProgramId,
                CourseId = student.CourseId
            };

            // Act

            var navigationAggregate = new NavigateUserAggregate(currentStateInfo, null, ptResults);

            var navigateAction = navigationAggregate.GetNavigateAction();
            // Assert

            Assert.NotNull(navigateAction);
            Assert.Equal(ptResults.First().Id, navigateAction.PtResultId);
            Assert.Equal(EnumNavigateActionStatus.ContinueLearning, navigateAction.Status);
        }

        // user are learning and have history (this case only happen when user just finish placement test and start learning in new system)
        [Fact]
        public void TestCase2()
        {
            var course = new Course
            {
                LevelId = Data.a1.Id,
                Level = new Level
                {
                    Id = Data.a1.Id,
                    ProgramId = Data.englishSubject.Categorys.ElementAt(0).Categorys.First(x => x.Name == "Academic English").Id
                }
            };

            // Arrange
            var student = new StudentModel { Id = Guid.NewGuid(), User = new UserModel { Birthday = new DateTime(2014, 1, 1), }, CourseId = course.Id };

            var ptResults = new List<TestGroupResult>
            {
                new TestGroupResult
                {
                    Id = Guid.NewGuid(),
                    StudentId = student.Id,
                    TestType = EnumTestType.PlacementTest,
                    CurrentLevelId = Data.a2.Id,
                    Percent = 85,
                    Status = EnumResultStatus.Done
                }
            };

            var currentCourseResult = new CourseResult
            {
                Id = Guid.NewGuid(),
                StudentId = student.Id,
                CourseId = student.CourseId.Value,
                TestGroupResults = ptResults,
                Course = course
            };

            var changeHistories = new List<CourseChangingHistory>
            {
                new CourseChangingHistory
                {
                    ToProgramId = Data.englishSubject.Categorys.ElementAt(0).Categorys.First(x => x.Name == "Academic English").Id,
                    SelectedLevelId = Data.a1.Id,
                    SelectedProgramId = Data.englishSubject.Categorys.ElementAt(0).Categorys.First(x => x.Name == "Academic English").Id,
                    PtResultId = ptResults.First().Id,
                    ToCourseResultId = currentCourseResult.Id,
                }
            };

            var currentStateInfo = new CurrentStateInfo
            {
                LevelId = currentCourseResult.Course.LevelId,
                CourseResultId = currentCourseResult.Id,
                ProgramId = currentCourseResult.Course.Level.ProgramId,
                CourseId = student.CourseId
            };

            // Act

            var navigationAggregate = new NavigateUserAggregate(currentStateInfo, changeHistories, ptResults);

            var navigateAction = navigationAggregate.GetNavigateAction();
            // Assert

            Assert.NotNull(navigateAction);
            Assert.Equal(ptResults.First().Id, navigateAction.PtResultId);
            Assert.Equal(EnumNavigateActionStatus.ContinueLearning, navigateAction.Status);
        }

        // nothing
        [Fact]
        public void TestCase3()
        {
            var currentStateInfo = new CurrentStateInfo
            {
                LevelId = null,
                CourseResultId = null,
                ProgramId = null,
                CourseId = null
            };

            // Act

            var navigationAggregate = new NavigateUserAggregate(currentStateInfo, null, null);

            var navigateAction = navigationAggregate.GetNavigateAction();
            // Assert

            Assert.NotNull(navigateAction);
            Assert.Equal(EnumNavigateActionStatus.NotDoingYetAnything, navigateAction.Status);
        }
    }
}
