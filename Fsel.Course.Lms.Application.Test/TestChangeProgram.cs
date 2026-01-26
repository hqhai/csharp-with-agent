// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Test
{
    using Domain.Entities;
    using Domain.Models.EntityModels.ChangeCourseModels;
    using Services.ApplicationServices.ChangeCourse;
    using Services.UserServices.Models;

    public class TestChangeProgram
    {
        // Chọn program khi chưa học gì, chọn Academic
        [Fact]
        public async Task TestCase1()
        {
            // Arrange
            var student = new StudentModel { Id = Guid.NewGuid(), User = new UserModel { Birthday = new DateTime(2014, 1, 1), }, CourseId = Guid.NewGuid() };

            var changeProgramRequest = new ChangeProgramRequest { ProgramId = Data.englishSubject.Categorys.ElementAt(0).Categorys.First(x => x.Name == "Academic English").Id };
            var changeCourseAggregateBuilder =
                new ChangeCourseAggregateBuilder(Data.subjects, null, null, null, student.User)
                    .BuildTree(null)
                    .BuildProgramInfo();

            var changeCourseAggregate = changeCourseAggregateBuilder.Build();

            // Act

            var directive = changeCourseAggregate.ChangeProgram(changeProgramRequest);

            // Assert
            Assert.NotNull(directive);
            Assert.Equivalent(directive.Action, EnumChangeProgramAction.ChangeAndStartPt);
            Assert.Equivalent(directive.ToProgramId, Data.englishSubject.Categorys.ElementAt(0).Categorys.First(x => x.Name == "Academic English").Id);
            Assert.Equivalent(directive.ProgramOwnPt, Data.englishSubject.Categorys.ElementAt(0).Categorys.First(x => x.Name == "Academic English").Id);
        }

        // Chọn program khi chưa học gì, chọn IELTS
        [Fact]
        public async Task TestCase2()
        {
            // Arrange
            var student = new StudentModel { Id = Guid.NewGuid(), User = new UserModel { Birthday = new DateTime(2014, 1, 1), }, CourseId = Guid.NewGuid() };

            var changeProgramRequest = new ChangeProgramRequest { ProgramId = Data.englishSubject.Categorys.ElementAt(0).Categorys.First(x => x.Name == "IELTS").Id };
            var changeCourseAggregateBuilder =
                new ChangeCourseAggregateBuilder(Data.subjects, null, null, null, student.User)
                    .BuildTree(null)
                    .BuildProgramInfo();

            var changeCourseAggregate = changeCourseAggregateBuilder.Build();

            // Act

            var directive = changeCourseAggregate.ChangeProgram(changeProgramRequest);

            // Assert
            Assert.NotNull(directive);
            Assert.Equivalent(directive.Action, EnumChangeProgramAction.ChangeAndStartPt);
            Assert.Equivalent(directive.ToProgramId, Data.englishSubject.Categorys.ElementAt(0).Categorys.First(x => x.Name == "IELTS").Id);
            Assert.Equivalent(directive.ProgramOwnPt, Data.englishSubject.Categorys.ElementAt(0).Categorys.First(x => x.Name == "Academic English").Id);
        }
    }
}
