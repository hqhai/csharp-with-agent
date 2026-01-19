// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Test
{
    using Domain.Entities;
    using Domain.Entities.TestConfigs;
    using Domain.Models.EntityModels.ChangeCourseModels;
    using Services.ApplicationServices.ChangeCourse;
    using Services.UserServices.Models;
    using Shared.Enums;

    public class TestChangeToNewCourseOtherProgram
    {
        // case1:
        // - Đang không học, chọn m1, pt đạt b2, chọn b1
        //
        // => Đầu vào cụ thể:
        // - Đang không học
        // - Tồn tại PT của program có level là B2, có history
        //
        // - Chọn đổi sang B1
        //
        //     => Đổi trực tiếp
        [Fact]
        public async Task TestCase1()
        {
            // Arrange
            var student = new StudentModel { Id = Guid.NewGuid(), User = new UserModel { Birthday = new DateTime(2014, 1, 1), }, CourseId = Guid.NewGuid() };
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
                            Action = EnumChangeCourseAction.ChangeAndStartPt,
                            Status = EnumChangingStatus.InProgressSelectCourse,
                        }
                    }
                }
            };

            var courseChangingHistories = ptResults.SelectMany(x => x.CourseChangingHistories)
                .Where(x => x.Status is EnumChangingStatus.Completed or EnumChangingStatus.InProgressSelectCourse)
                .ToList();

            var selectedLevel = Data.b1.Id;

            var changeLevelRequest = new ChangeCourseRequest { LevelId = selectedLevel, RequestType = EnumChangeCourseRequest.ChangeCourse, };

            var changeCourseAggregateBuilder =
                new ChangeCourseAggregateBuilder(Data.subjects, ptResults, courseChangingHistories, null, student.User)
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
            Assert.Equivalent(directive.Action, EnumChangeCourseAction.ChangeDirectly);
            Assert.Equivalent(directive.ToProgramId, Data.englishSubject.Categorys.ElementAt(0).Categorys.First(x => x.Name == "Academic English").Id);
            Assert.Equivalent(directive.ToLevelId, changeLevelRequest.LevelId);
        }

        // case2:
        // - Đang học A1, Đổi sang IELTS m1
        // - Vừa làm pt xong được level B2
        // - Chọn level m2
        //
        // => Đầu vào cụ thể:
        // - Đang không học
        // - Tồn tại PT của program có level là B2, có history
        //
        //     => Đổi trực tiếp
        [Fact]
        public async Task TestCase2()
        {
            // Arrange
            var student = new StudentModel { Id = Guid.NewGuid(), User = new UserModel { Birthday = new DateTime(2009, 1, 1), }, CourseId = Guid.NewGuid() };
            var courseResult = new CourseResult()
            {
                Id = Guid.NewGuid(),
                StudentId = student.Id,
                Course = new Course { Id = student.CourseId.Value, LevelId = Data.a1.Id },
                WorkingStatus = EnumWorkingStatus.Active,
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
                            Action = EnumChangeCourseAction.ChangeAndStartPt,
                            Status = EnumChangingStatus.InProgressSelectCourse,
                        }
                    }
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    StudentId = student.Id,
                    ProgramId = Data.englishSubject.Categorys.ElementAt(0).Categorys.First(x => x.Name == "Academic English").Id,
                    CurrentLevelId = Data.preA1.Id,
                    ProgramIdOfPt = Data.englishSubject.Categorys.ElementAt(0).Categorys.First(x => x.Name == "Academic English").Id,
                    CourseChangingHistories = new List<CourseChangingHistory>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            ToLevelId = Data.a1.Id,
                            ToProgramId = Data.englishSubject.Categorys.ElementAt(0).Categorys.First(x => x.Name == "Academic English").Id,
                            SelectedLevelId = Data.a1.Id,
                            SelectedProgramId = Data.englishSubject.Categorys.ElementAt(0).Categorys.First(x => x.Name == "Academic English").Id,
                            ToCourseResultId = courseResult.Id,
                            Action = EnumChangeCourseAction.ChangeAndStartPt,
                            Status = EnumChangingStatus.Completed,
                        }
                    }
                }
            };

            var courseChangingHistories = ptResults.SelectMany(x => x.CourseChangingHistories)
                .Where(x => x.Status is EnumChangingStatus.Completed or EnumChangingStatus.InProgressSelectCourse)
                .ToList();

            var selectedLevel = Data.m1.Id;

            var changeLevelRequest = new ChangeCourseRequest { LevelId = selectedLevel, RequestType = EnumChangeCourseRequest.ChangeCourse, };

            var changeCourseAggregateBuilder =
                new ChangeCourseAggregateBuilder(Data.subjects, ptResults, courseChangingHistories, courseResult, student.User)
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
            Assert.Equivalent(directive.Action, EnumChangeCourseAction.ChangeDirectly);
            Assert.Equivalent(directive.ToProgramId, Data.englishSubject.Categorys.ElementAt(0).Categorys.First(x => x.Name == "IELTS").Id);
            Assert.Equivalent(directive.ToLevelId, changeLevelRequest.LevelId);
        }

        // case3:
        // - Đang không học, chọn m1, pt đạt A1, chọn b1
        //
        // => Đầu vào cụ thể:
        // - Đang không học
        // - Tồn tại PT của program có level là A1, có history
        //
        // - Chọn đổi sang B1
        //
        //     => Cần làm PT
        [Fact]
        public async Task TestCase3()
        {
            // Arrange
            var student = new StudentModel { Id = Guid.NewGuid(), User = new UserModel { Birthday = new DateTime(2014, 1, 1), }, CourseId = Guid.NewGuid() };
            var ptResults = new List<TestGroupResult>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    StudentId = student.Id,
                    ProgramId = Data.englishSubject.Categorys.ElementAt(0).Categorys.First(x => x.Name == "IELTS").Id,
                    CurrentLevelId = Data.a1.Id,
                    ProgramIdOfPt = Data.englishSubject.Categorys.ElementAt(0).Categorys.First(x => x.Name == "Academic English").Id,
                    CourseChangingHistories = new List<CourseChangingHistory>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            ToLevelId = Data.m1.Id,
                            ToProgramId = Data.englishSubject.Categorys.ElementAt(0).Categorys.First(x => x.Name == "IELTS").Id,
                            Action = EnumChangeCourseAction.ChangeAndStartPt,
                            Status = EnumChangingStatus.InProgressSelectCourse,
                        }
                    }
                }
            };

            var courseChangingHistories = ptResults.SelectMany(x => x.CourseChangingHistories)
                .Where(x => x.Status is EnumChangingStatus.Completed or EnumChangingStatus.InProgressSelectCourse)
                .ToList();

            var selectedLevel = Data.b1.Id;

            var changeLevelRequest = new ChangeCourseRequest { LevelId = selectedLevel, RequestType = EnumChangeCourseRequest.ChangeCourse, };

            var changeCourseAggregateBuilder =
                new ChangeCourseAggregateBuilder(Data.subjects, ptResults, courseChangingHistories, null, student.User)
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
            Assert.Equivalent(directive.Action, EnumChangeCourseAction.ChangeAndStartPt);
            Assert.Equivalent(directive.ToProgramId, Data.englishSubject.Categorys.ElementAt(0).Categorys.First(x => x.Name == "Academic English").Id);
            Assert.Equivalent(directive.ProgramOwnPt, Data.englishSubject.Categorys.ElementAt(0).Categorys.First(x => x.Name == "Academic English").Id);
            Assert.Equivalent(directive.ToLevelId, changeLevelRequest.LevelId);
        }

        // case3:
        // - Đang  học A1, chọn m1
        //
        // => Đầu vào cụ thể:
        // - Tồn tại PT của program có level là A1, có history
        //
        // - Chọn đổi sang m1
        //
        //     => Cần làm PT
        [Fact]
        public async Task TestCase4()
        {
            // Arrange
            var student = new StudentModel { Id = Guid.NewGuid(), User = new UserModel { Birthday = new DateTime(2014, 1, 1), }, CourseId = Guid.NewGuid() };
            var ptResults = new List<TestGroupResult>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    StudentId = student.Id,
                    ProgramId = Data.englishSubject.Categorys.ElementAt(0).Categorys.First(x => x.Name == "Academic English").Id,
                    CurrentLevelId = Data.a1.Id,
                    ProgramIdOfPt = Data.englishSubject.Categorys.ElementAt(0).Categorys.First(x => x.Name == "Academic English").Id,
                    CourseChangingHistories = new List<CourseChangingHistory>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            SelectedLevelId = Data.a1.Id,
                            ToProgramId = Data.englishSubject.Categorys.ElementAt(0).Categorys.First(x => x.Name == "Academic English").Id,
                            SelectedProgramId = Data.englishSubject.Categorys.ElementAt(0).Categorys.First(x => x.Name == "Academic English").Id,
                            Action = EnumChangeCourseAction.ChangeAndStartPt,
                            Status = EnumChangingStatus.Completed,
                        }
                    }
                }
            };

            var courseChangingHistories = ptResults.SelectMany(x => x.CourseChangingHistories)
                .Where(x => x.Status is EnumChangingStatus.Completed or EnumChangingStatus.InProgressSelectCourse)
                .ToList();

            var selectedLevel = Data.m1.Id;

            var changeLevelRequest = new ChangeCourseRequest { LevelId = selectedLevel, RequestType = EnumChangeCourseRequest.ChangeCourse, };

            var changeCourseAggregateBuilder =
                new ChangeCourseAggregateBuilder(Data.subjects, ptResults, courseChangingHistories, null, student.User)
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
            Assert.Equivalent(directive.Action, EnumChangeCourseAction.ChangeAndStartPt);
            Assert.Equivalent(directive.ToProgramId, Data.englishSubject.Categorys.ElementAt(0).Categorys.First(x => x.Name == "IELTS").Id);
            Assert.Equivalent(directive.ProgramOwnPt, Data.englishSubject.Categorys.ElementAt(0).Categorys.First(x => x.Name == "Academic English").Id);
            Assert.Equivalent(directive.ToLevelId, changeLevelRequest.LevelId);
        }
    }
}
