// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Test
{
    using Domain.Entities;
    using Domain.Entities.TestConfigs;
    using Domain.Models.EntityModels.ChangeCourseModels;
    using Services.ApplicationServices.ChangeCourse;
    using Services.UserServices.Models;
    using Shared.Enums;

    public class TestChangeToExistCourseOtherProgram
    {
        //Đang học IELTS m1 muốn học lại B1 trước đó đã học
        [Fact]
        public async Task TestCase1()
        {
            // Arrange
            var student = new StudentModel { Id = Guid.NewGuid(), User = new UserModel { Birthday = new DateTime(2014, 1, 1), }, CourseId = Guid.NewGuid() };
            var courseM1Result = new CourseResult()
            {
                Id = Guid.NewGuid(),
                StudentId = student.Id,
                Course = new Course { Id = student.CourseId.Value, LevelId = Data.m1.Id },
                WorkingStatus = EnumWorkingStatus.Active,
            };

            var courseB1Result = new CourseResult()
            {
                Id = Guid.NewGuid(),
                StudentId = student.Id,
                Course = new Course { Id = student.CourseId.Value, LevelId = Data.b1.Id },
                WorkingStatus = EnumWorkingStatus.InActive,
            };
            var ptResults = new List<TestGroupResult>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    StudentId = student.Id,
                    ProgramId = Data.englishSubject.Categorys.ElementAt(0).Categorys.First(x => x.Name == "IELTS").Id,
                    CurrentLevelId = Data.b2.Id,
                    ProgramIdOfPt = Data.englishSubject.Categorys.ElementAt(0).Categorys.First(x => x.Name == "Academic English").Id,
                    CourseChangingHistories = new List<CourseChangingHistory>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            ToLevelId = Data.m1.Id,
                            ToProgramId = Data.englishSubject.Categorys.ElementAt(0).Categorys.First(x => x.Name == "IELTS").Id,
                            SelectedLevelId = Data.m1.Id,
                            SelectedProgramId = Data.englishSubject.Categorys.ElementAt(0).Categorys.First(x => x.Name == "IELTS").Id,
                            ToCourseResultId = courseM1Result.Id,
                            Action = EnumChangeCourseAction.ChangeDirectly,
                            Status = EnumChangingStatus.Completed,
                        },
                        new()
                        {
                            Id = Guid.NewGuid(),
                            ToLevelId = Data.m1.Id,
                            ToProgramId = Data.englishSubject.Categorys.ElementAt(0).Categorys.First(x => x.Name == "IELTS").Id,
                            SelectedLevelId = Data.b1.Id,
                            SelectedProgramId = Data.englishSubject.Categorys.ElementAt(0).Categorys.First(x => x.Name == "Academic English").Id,
                            ToCourseResultId = courseB1Result.Id,
                            Action = EnumChangeCourseAction.ChangeAndStartPt,
                            Status = EnumChangingStatus.Completed,
                        },
                    }
                }
            };

            var courseChangingHistories = ptResults.SelectMany(x => x.CourseChangingHistories)
                .Where(x => x.Status is EnumChangingStatus.Completed or EnumChangingStatus.InProgressSelectCourse)
                .ToList();

            var selectedLevel = Data.b1.Id;

            var changeLevelRequest = new ChangeCourseRequest { LevelId = selectedLevel, RequestType = EnumChangeCourseRequest.ChangeCourse, };

            var changeCourseAggregateBuilder =
                new ChangeCourseAggregateBuilder(Data.subjects, ptResults, courseChangingHistories, courseM1Result, student.User)
                    .BuildTree(selectedLevel)
                    .BuildProgramInfo()
                    .BuildLevelInfomation();
            await changeCourseAggregateBuilder.BuildLevelAccess(async _ =>
            {
                await Task.Yield();
                return Data.englishSubjectConditions.SelectMany(x => x.SubjectConditionRules).ToList();
            });

            var changeCourseAggregate = changeCourseAggregateBuilder.Build();

            // Act

            var directive = changeCourseAggregate.ChangeCourse(changeLevelRequest);

            // Assert
            Assert.NotNull(directive);
            Assert.Equivalent(directive.Action, EnumChangeCourseAction.SwitchToExistedCourse);
            Assert.Equivalent(directive.CourseResultId, courseB1Result.Id);
        }
    }
}
