// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Test
{
    using Domain.Entities;
    using Domain.Entities.TestConfigs;
    using Domain.Models.EntityModels.ChangeCourseModels;
    using Services.ApplicationServices.ChangeCourse;
    using Services.UserServices.Models;
    using Shared.Enums;

    public class TestChangeToNewCourseSameProgram
    {
        /// <summary>
        /// case1:
        // - Đang học
        //     - Từ level cao hơn xuống level thấp hơn
        //     - Cùng Program
        //
        //     => Đầu vào cụ thể:
        // - Đang học A2
        //     - Tồn tại PT của program, Có thể có history
        //     - Tồn tại CourseResult của Level A2
        //     - Chọn đổi sang A1
        //
        //     => Đổi trực tiếp, không cần pt
        /// </summary>
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
                    ProgramId = Data.englishSubject.Categorys.ElementAt(0).Categorys.First(x => x.Name == "Academic English").Id,
                    CurrentLevelId = Data.a2.Id,
                    ProgramIdOfPt = Data.englishSubject.Categorys.ElementAt(0).Categorys.First(x => x.Name == "Academic English").Id,
                }
            };

            var courseResult = new CourseResult()
            {
                Id = Guid.NewGuid(),
                StudentId = student.Id,
                Course = new Course { Id = student.CourseId.Value, LevelId = Data.a2.Id },
                WorkingStatus = EnumWorkingStatus.Active,
            };

            var courseChangingHistories = ptResults.SelectMany(x => x.CourseChangingHistories)
                .Where(x => x.Status is EnumChangingStatus.Completed or EnumChangingStatus.InProgressSelectCourse)
                .ToList();

            var selectedLevel = Data.a1.Id;

            var changeLevelRequest = new ChangeCourseRequest { LevelId = Data.a1.Id, RequestType = EnumChangeCourseRequest.ChangeCourse, };

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
            Assert.Equivalent(directive.ToProgramId, Data.englishSubject.Categorys.ElementAt(0).Categorys.First(x => x.Name == "Academic English").Id);
            Assert.Equivalent(directive.ToLevelId, changeLevelRequest.LevelId);
        }

        // case2:
        // - Đang học
        // - Từ level cao hơn xuống level thấp hơn
        // - Cùng Program
        //
        //     => Đầu vào cụ thể:
        // - Đang học A2
        // - Tồn tại PT của program, có history
        // - Tồn tại CourseResult của Level A2
        // - Chọn đổi sang A1
        //
        //     => Đổi trực tiếp, không cần pt

        [Fact]
        public async Task TestCase2()
        {
            // Arrange
            var student = new StudentModel { Id = Guid.NewGuid(), User = new UserModel { Birthday = new DateTime(2014, 1, 1), }, CourseId = Guid.NewGuid() };
            var courseResult = new CourseResult()
            {
                Id = Guid.NewGuid(),
                StudentId = student.Id,
                Course = new Course { Id = student.CourseId.Value, LevelId = Data.a2.Id },
                WorkingStatus = EnumWorkingStatus.Active,
            };
            var ptResults = new List<TestGroupResult>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    StudentId = student.Id,
                    ProgramId = Data.englishSubject.Categorys.ElementAt(0).Categorys.First(x => x.Name == "Academic English").Id,
                    CurrentLevelId = Data.a2.Id,
                    ProgramIdOfPt = Data.englishSubject.Categorys.ElementAt(0).Categorys.First(x => x.Name == "Academic English").Id,
                    CourseChangingHistories = new List<CourseChangingHistory>
                    {
                        new CourseChangingHistory
                        {
                            Id = Guid.NewGuid(),
                            ToLevelId = Data.a2.Id,
                            ToProgramId = Data.englishSubject.Categorys.ElementAt(0).Categorys.First(x => x.Name == "Academic English").Id,
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

            var selectedLevel = Data.a1.Id;

            var changeLevelRequest = new ChangeCourseRequest { LevelId = Data.a1.Id, RequestType = EnumChangeCourseRequest.ChangeCourse, };

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
            Assert.Equivalent(directive.ToProgramId, Data.englishSubject.Categorys.ElementAt(0).Categorys.First(x => x.Name == "Academic English").Id);
            Assert.Equivalent(directive.ToLevelId, changeLevelRequest.LevelId);
        }

        //
        // case3:
        // - Đang học
        // - Từ level thấp hơn lên level cao hơn với pt cũ thỏa mãn
        // - Cùng Program
        //
        //     => Đầu vào cụ thể:
        // - Đang học A1
        // - Tồn tại PT của program có level là A1, có history
        // - Tồn tại CourseResult của Level A1
        // - Chọn đổi sang A2
        //
        //     => Đổi trực tiếp, không cần pt
        [Fact]
        public async Task TestCase3()
        {
            // Arrange
            var student = new StudentModel { Id = Guid.NewGuid(), User = new UserModel { Birthday = new DateTime(2014, 1, 1), }, CourseId = Guid.NewGuid() };
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
                    ProgramId = Data.englishSubject.Categorys.ElementAt(0).Categorys.First(x => x.Name == "Academic English").Id,
                    CurrentLevelId = Data.a1.Id,
                    ProgramIdOfPt = Data.englishSubject.Categorys.ElementAt(0).Categorys.First(x => x.Name == "Academic English").Id,
                    CourseChangingHistories = new List<CourseChangingHistory>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            ToLevelId = Data.a1.Id,
                            ToProgramId = Data.englishSubject.Categorys.ElementAt(0).Categorys.First(x => x.Name == "Academic English").Id,
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

            var selectedLevel = Data.a2.Id;

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
            Assert.Equivalent(directive.ToProgramId, Data.englishSubject.Categorys.ElementAt(0).Categorys.First(x => x.Name == "Academic English").Id);
            Assert.Equivalent(directive.ToLevelId, changeLevelRequest.LevelId);
        }

        // case4:
        // - Đang học
        // - Từ level thấp hơn lên level cao hơn với pt cũ khoông thỏa mãn
        // - Cùng Program
        //
        //     => Đầu vào cụ thể:
        // - Đang học A1
        // - Tồn tại PT của program có level là PreA1, có history
        // - Tồn tại CourseResult của Level A1
        // - Chọn đổi sang A2
        //
        //     => Đổi trực tiếp, không cần pt
        [Fact]
        public async Task TestCase4()
        {
            // Arrange
            var student = new StudentModel { Id = Guid.NewGuid(), User = new UserModel { Birthday = new DateTime(2014, 1, 1), }, CourseId = Guid.NewGuid() };
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

            var selectedLevel = Data.b1.Id;

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
            Assert.Equivalent(directive.Action, EnumChangeCourseAction.ChangeAndStartPt);
            Assert.Equivalent(directive.ToProgramId, Data.englishSubject.Categorys.ElementAt(0).Categorys.First(x => x.Name == "Academic English").Id);
            Assert.Equivalent(directive.ToLevelId, changeLevelRequest.LevelId);
        }

        // case5:
        // - Vừa làm pt xong được level B1
        // - Chọn level B1
        //
        // => Đầu vào cụ thể:
        // - Đang không học
        // - Tồn tại PT của program có level là B1, có history
        //
        // - Chọn đổi sang B1
        //
        //     => Đổi trực tiếp

        [Fact]
        public async Task TestCase5()
        {
            // Arrange
            var student = new StudentModel { Id = Guid.NewGuid(), User = new UserModel { Birthday = new DateTime(2014, 1, 1), }, CourseId = Guid.NewGuid() };
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
                    ProgramId = Data.englishSubject.Categorys.ElementAt(0).Categorys.First(x => x.Name == "Academic English").Id,
                    CurrentLevelId = Data.b1.Id,
                    ProgramIdOfPt = Data.englishSubject.Categorys.ElementAt(0).Categorys.First(x => x.Name == "Academic English").Id,
                    CourseChangingHistories = new List<CourseChangingHistory>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            ToLevelId = Data.b1.Id,
                            ToProgramId = Data.englishSubject.Categorys.ElementAt(0).Categorys.First(x => x.Name == "Academic English").Id,
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
    }
}
