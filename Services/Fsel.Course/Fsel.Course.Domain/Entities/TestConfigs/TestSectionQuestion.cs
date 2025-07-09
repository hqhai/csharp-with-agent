// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.TestConfigs
{
    using Fsel.Core.Entities;

    public class TestSectionQuestion : Entity
    {
        public Guid QuestionId { get; set; }
        public Question Question { get; set; } = new Question();

        public Guid TestSectionId { get; set; }
        public TestSection TestSection { get; set; } = new TestSection();
    }
}
