// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.TestConfig
{
    using Fsel.Core.Entities;

    public class TestConfigSectionQuestion : Entity
    {
        public Guid QuestionId { get; set; }
        public Question Question { get; set; } = new Question();

        public Guid TestConfigSectionId { get; set; }
        public TestConfigSection TestConfigSection { get; set; } = new TestConfigSection();
    }
}
