// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Test
{
    using Domain.Entities;
    using Domain.Entities.SubjectConditionRuleConfigs;
    using Domain.Enums;
    using Shared.Enums;

    public static class Data
    {
        public static Level preA1 = new Level() { Id = new Guid("2DBE407C-D9D2-4837-A0BE-9CC89E5D12BE"), Name = "Pre A1", };
        public static Level a1 = new Level { Id = new Guid("E14C96EC-A571-4E13-9900-03D9362FE54A"), Name = "A1", LevelOrder = 0 };
        public static Level a2 = new Level { Id = new Guid("5864FB49-DD6E-4605-A0A9-793270E2792F"), Name = "A2", LevelOrder = 1 };
        public static Level b1 = new Level { Id = new Guid("4926199C-8B8A-445E-A1B8-363F7C6FA3DB"), Name = "B1", LevelOrder = 2 };
        public static Level b1Plus = new Level { Id = new Guid("7C56E9DD-EBF4-4706-A6A8-37042122E197"), Name = "B1 Plus", LevelOrder = 3 };
        public static Level b2 = new Level { Id = new Guid("DA783BFD-4D8B-48F9-80F6-9391A8716268"), Name = "B2", LevelOrder = 4 };
        public static Level c1 = new Level { Id = new Guid("406F099A-2C19-4D8A-8D04-10F95740CAD7"), Name = "C1", LevelOrder = 5 };

        public static Level fA1 = new Level { Id = new Guid("F605C29C-C526-4D3C-AF93-9B90FAD16A0B"), Name = "FA1", LevelOrder = 0 };
        public static Level fA2 = new Level { Id = new Guid("8DF322A7-A463-44C8-BA15-382451807E67"), Name = "FA2", LevelOrder = 1 };
        public static Level fB1 = new Level { Id = new Guid("CFB4DBB1-3FDF-4FA6-B4B1-AF99B4081328"), Name = "FB1", LevelOrder = 2 };

        public static Level m1 = new Level { Id = new Guid("D8953C3B-E5B3-4257-9891-34E1654F40FE"), Name = "IELTS - Mindset 1 -  5.", LevelOrder = 0 };
        public static Level m2 = new Level { Id = new Guid("70BB894B-BFCD-4A29-B951-91E35364EF0C"), Name = "IELTS - Mindset 2 -  6.", LevelOrder = 1 };
        public static Level m3 = new Level { Id = new Guid("8AD1DA27-49DF-4745-9EEB-E3F415648015"), Name = "IELTS - Mindset 3 -  7.", LevelOrder = 2 };

        public static Level n5 = new Level { Id = new Guid("A93A6997-3421-481F-9E59-99C03762C6B0"), Name = "N5", LevelOrder = 0 };
        public static Level n4 = new Level { Id = new Guid("D5290819-0D95-434A-A21C-A258517C749B"), Name = "N4", LevelOrder = 1 };
        public static Level n3 = new Level { Id = new Guid("5EBC6F77-C63B-454D-A958-06661A000AC1"), Name = "N3", LevelOrder = 2 };

        public static Category englishSubject = new()
        {
            Id = new Guid("f4bcb55c-50a9-4878-9e69-46b15dc9220b"),
            Name = "English",
            Type = EnumTypeCategory.Subject,
            Categorys = new List<Category>
            {
                // project
                new()
                {
                    Id = new Guid("79df7978-d37e-485d-9122-dd3942d63f9d"),
                    ParentId = new Guid("f4bcb55c-50a9-4878-9e69-46b15dc9220b"),
                    Name = "English",
                    Type = EnumTypeCategory.Subject,
                    Categorys = new List<Category>
                    {
                        new()
                        {
                            Id = new Guid("b860d80a-ecf2-4dbc-87db-7df42d62a29e"),
                            ParentId = new Guid("79df7978-d37e-485d-9122-dd3942d63f9d"),
                            Name = "Foundation English",
                            Type = EnumTypeCategory.Program,
                            Levels =
                                new List<Level> { fA1, fA2, fB1 },
                            TestMode = EnumTestMode.Default
                        },
                        new()
                        {
                            Id = new Guid("233e7bdd-4539-49e3-b493-7aef806ed82b"),
                            ParentId = new Guid("79df7978-d37e-485d-9122-dd3942d63f9d"),
                            Name = "Academic English",
                            Type = EnumTypeCategory.Program,
                            Levels =
                                new List<Level>
                                {
                                    preA1,
                                    a1,
                                    a2,
                                    b1,
                                    b1Plus,
                                    b2
                                },
                            TestMode = EnumTestMode.Custom,
                            IsTestDefault = true
                        },
                        new()
                        {
                            Id = new Guid("8ed06516-05b2-42ed-818d-69d78c05bcde"),
                            ParentId = new Guid("79df7978-d37e-485d-9122-dd3942d63f9d"),
                            Name = "IELTS",
                            Type = EnumTypeCategory.Program,
                            Levels = new List<Level> { m1, m2, m3 },
                            TestMode = EnumTestMode.Default
                        }
                    },
                    SubjectConditions = englishSubjectConditions
                }
            }
        };

        public static Category japanSubject = new Category
        {
            Id = new Guid("b6c79138-9184-4c65-8452-1d990e1707a9"),
            Name = "SubjectJPN",
            Type = EnumTypeCategory.Subject,
            Categorys = new List<Category>
            {
                // project
                new()
                {
                    Id = new Guid("f35bbc1d-3698-4fb5-a265-3c338f96f580"),
                    ParentId = new Guid("b6c79138-9184-4c65-8452-1d990e1707a9"),
                    Name = "Japanese",
                    Type = EnumTypeCategory.Subject,
                    Categorys = new List<Category>
                    {
                        new()
                        {
                            Id = new Guid("b912e418-ee6e-458f-9a35-ba1bb5849589"),
                            ParentId = new Guid("f35bbc1d-3698-4fb5-a265-3c338f96f580"),
                            Name = "Japanese",
                            Type = EnumTypeCategory.Program,
                            Levels =
                                new List<Level> { n5, n4, n3 }
                        }
                    },
                    SubjectConditions = englishSubjectConditions
                }
            }
        };

        public static List<SubjectCondition> englishSubjectConditions = new()
        {
            new()
            {
                Type = EnumConditionType.CourseSuggest,
                SubjectConditionRules =
                    new List<SubjectConditionRule>
                    {
                        new()
                        {
                            ConditionRules = new List<ConditionRule>
                            {
                                new() { Type = EnumSubjectConditionRuleType.Age, FromAge = 16, ToAge = null, OperatorType = EnumOperatorType.GreaterThanEqual },
                                new() { Type = EnumSubjectConditionRuleType.CurrentLevel, LevelIds = new[] { b1.Id }, OperatorType = EnumOperatorType.Include }
                            },
                            ConditionValues =
                                new List<ConditionValue>
                                {
                                    new() { Type = EnumSubjectConditionValueType.Basic, LevelIds = new[] { a2.Id, fA2.Id }, },
                                    new() { Type = EnumSubjectConditionValueType.Recommended, LevelIds = new[] { b1.Id, fB1.Id }, },
                                    new() { Type = EnumSubjectConditionValueType.Recommended, LevelIds = new[] { m1.Id, b1Plus.Id }, }
                                }
                        },
                        new()
                        {
                            ConditionRules = new List<ConditionRule>
                            {
                                new() { Type = EnumSubjectConditionRuleType.Age, FromAge = 14, ToAge = null, OperatorType = EnumOperatorType.LessThan },
                                new() { Type = EnumSubjectConditionRuleType.CurrentLevel, LevelIds = new[] { b1Plus.Id }, OperatorType = EnumOperatorType.Include }
                            },
                            ConditionValues =
                                new List<ConditionValue>
                                {
                                    new() { Type = EnumSubjectConditionValueType.Basic, LevelIds = new[] { b1.Id }, },
                                    new() { Type = EnumSubjectConditionValueType.Recommended, LevelIds = new[] { b1Plus.Id }, },
                                    new() { Type = EnumSubjectConditionValueType.Recommended, LevelIds = new[] { b2.Id }, }
                                }
                        },
                        new()
                        {
                            ConditionRules = new List<ConditionRule>
                            {
                                new() { Type = EnumSubjectConditionRuleType.Age, FromAge = 14, ToAge = null, OperatorType = EnumOperatorType.LessThan },
                                new() { Type = EnumSubjectConditionRuleType.CurrentLevel, LevelIds = new[] { b1.Id }, OperatorType = EnumOperatorType.Include }
                            },
                            ConditionValues =
                                new List<ConditionValue>
                                {
                                    new() { Type = EnumSubjectConditionValueType.Basic, LevelIds = new[] { a2.Id }, },
                                    new() { Type = EnumSubjectConditionValueType.Recommended, LevelIds = new[] { b1.Id }, },
                                    new() { Type = EnumSubjectConditionValueType.Recommended, LevelIds = new[] { b1Plus.Id }, }
                                }
                        },
                        new()
                        {
                            ConditionRules = new List<ConditionRule>
                            {
                                new() { Type = EnumSubjectConditionRuleType.Age, FromAge = 14, ToAge = 16, OperatorType = EnumOperatorType.Between },
                                new() { Type = EnumSubjectConditionRuleType.CurrentLevel, LevelIds = new[] { b1.Id }, OperatorType = EnumOperatorType.Include }
                            },
                            ConditionValues =
                                new List<ConditionValue>
                                {
                                    new() { Type = EnumSubjectConditionValueType.Basic, LevelIds = new[] { a2.Id }, },
                                    new() { Type = EnumSubjectConditionValueType.Recommended, LevelIds = new[] { b1.Id }, },
                                    new() { Type = EnumSubjectConditionValueType.Recommended, LevelIds = new[] { m1.Id, b1Plus.Id }, }
                                }
                        },
                        new()
                        {
                            ConditionRules = new List<ConditionRule>
                            {
                                new() { Type = EnumSubjectConditionRuleType.Age, FromAge = 14, OperatorType = EnumOperatorType.LessThan },
                                new() { Type = EnumSubjectConditionRuleType.CurrentLevel, LevelIds = new[] { a2.Id }, OperatorType = EnumOperatorType.Include }
                            },
                            ConditionValues =
                                new List<ConditionValue>
                                {
                                    new() { Type = EnumSubjectConditionValueType.Basic, LevelIds = new[] { a1.Id }, },
                                    new() { Type = EnumSubjectConditionValueType.Recommended, LevelIds = new[] { a2.Id, }, },
                                    new() { Type = EnumSubjectConditionValueType.Recommended, LevelIds = new[] { b1.Id }, }
                                }
                        },
                        new()
                        {
                            ConditionRules = new List<ConditionRule>
                            {
                                new() { Type = EnumSubjectConditionRuleType.Age, FromAge = 16, OperatorType = EnumOperatorType.GreaterThanEqual },
                                new() { Type = EnumSubjectConditionRuleType.CurrentLevel, LevelIds = new[] { b1Plus.Id }, OperatorType = EnumOperatorType.Include }
                            },
                            ConditionValues =
                                new List<ConditionValue>
                                {
                                    new() { Type = EnumSubjectConditionValueType.Basic, LevelIds = new[] { b1.Id, fB1.Id }, },
                                    new() { Type = EnumSubjectConditionValueType.Recommended, LevelIds = new[] { b1Plus.Id, m1.Id }, },
                                    new() { Type = EnumSubjectConditionValueType.Recommended, LevelIds = new[] { m2.Id }, }
                                }
                        },
                        new()
                        {
                            ConditionRules = new List<ConditionRule>
                            {
                                new() { Type = EnumSubjectConditionRuleType.Age, FromAge = 16, OperatorType = EnumOperatorType.GreaterThanEqual },
                                new() { Type = EnumSubjectConditionRuleType.CurrentLevel, LevelIds = new[] { b2.Id }, OperatorType = EnumOperatorType.Include }
                            },
                            ConditionValues =
                                new List<ConditionValue>
                                {
                                    new() { Type = EnumSubjectConditionValueType.Basic, LevelIds = new[] { m1.Id, b1Plus.Id }, },
                                    new() { Type = EnumSubjectConditionValueType.Recommended, LevelIds = new[] { b2.Id, m2.Id }, },
                                    new() { Type = EnumSubjectConditionValueType.Advanced, LevelIds = new[] { c1.Id, m3.Id }, }
                                }
                        },
                        new()
                        {
                            ConditionRules = new List<ConditionRule>
                            {
                                new() { Type = EnumSubjectConditionRuleType.Age, FromAge = 14, ToAge = 16, OperatorType = EnumOperatorType.Between },
                                new() { Type = EnumSubjectConditionRuleType.CurrentLevel, LevelIds = new[] { b2.Id }, OperatorType = EnumOperatorType.Include }
                            },
                            ConditionValues =
                                new List<ConditionValue>
                                {
                                    new() { Type = EnumSubjectConditionValueType.Basic, LevelIds = new[] { m1.Id, b1Plus.Id }, },
                                    new() { Type = EnumSubjectConditionValueType.Recommended, LevelIds = new[] { b2.Id, m2.Id }, },
                                    new() { Type = EnumSubjectConditionValueType.Advanced, LevelIds = new[] { c1.Id, m3.Id }, }
                                }
                        },
                        new()
                        {
                            ConditionRules = new List<ConditionRule>
                            {
                                new() { Type = EnumSubjectConditionRuleType.Age, FromAge = 16, OperatorType = EnumOperatorType.GreaterThanEqual },
                                new() { Type = EnumSubjectConditionRuleType.CurrentLevel, LevelIds = new[] { preA1.Id }, OperatorType = EnumOperatorType.Include }
                            },
                            ConditionValues =
                                new List<ConditionValue> { new() { Type = EnumSubjectConditionValueType.Basic, LevelIds = new[] { a1.Id, } } },
                        },
                        new()
                        {
                            ConditionRules = new List<ConditionRule>
                            {
                                new() { Type = EnumSubjectConditionRuleType.Age, FromAge = 14, ToAge = 16, OperatorType = EnumOperatorType.Between },
                                new() { Type = EnumSubjectConditionRuleType.CurrentLevel, LevelIds = new[] { a2.Id }, OperatorType = EnumOperatorType.Include }
                            },
                            ConditionValues =
                                new List<ConditionValue>
                                {
                                    new() { Type = EnumSubjectConditionValueType.Basic, LevelIds = new[] { a1.Id, } },
                                    new() { Type = EnumSubjectConditionValueType.Recommended, LevelIds = new[] { a2.Id, } },
                                    new() { Type = EnumSubjectConditionValueType.Advanced, LevelIds = new[] { b1.Id, } }
                                },
                        },
                        new()
                        {
                            ConditionRules = new List<ConditionRule>
                            {
                                new() { Type = EnumSubjectConditionRuleType.Age, FromAge = 14, ToAge = null, OperatorType = EnumOperatorType.LessThan },
                                new() { Type = EnumSubjectConditionRuleType.CurrentLevel, LevelIds = new[] { preA1.Id }, OperatorType = EnumOperatorType.Include }
                            },
                            ConditionValues =
                                new List<ConditionValue> { new() { Type = EnumSubjectConditionValueType.Basic, LevelIds = new[] { a1.Id, } }, },
                        },
                        new()
                        {
                            ConditionRules = new List<ConditionRule>
                            {
                                new() { Type = EnumSubjectConditionRuleType.Age, FromAge = 14, ToAge = 16, OperatorType = EnumOperatorType.Between },
                                new() { Type = EnumSubjectConditionRuleType.CurrentLevel, LevelIds = new[] { preA1.Id }, OperatorType = EnumOperatorType.Include }
                            },
                            ConditionValues =
                                new List<ConditionValue> { new() { Type = EnumSubjectConditionValueType.Basic, LevelIds = new[] { a1.Id, } }, },
                        },
                        new()
                        {
                            ConditionRules = new List<ConditionRule>
                            {
                                new() { Type = EnumSubjectConditionRuleType.Age, FromAge = 16, ToAge = null, OperatorType = EnumOperatorType.GreaterThanEqual },
                                new() { Type = EnumSubjectConditionRuleType.CurrentLevel, LevelIds = new[] { c1.Id }, OperatorType = EnumOperatorType.Include }
                            },
                            ConditionValues =
                                new List<ConditionValue>
                                {
                                    new() { Type = EnumSubjectConditionValueType.Basic, LevelIds = new[] { b2.Id, m2.Id } },
                                    new() { Type = EnumSubjectConditionValueType.Basic, LevelIds = new[] { c1.Id, m3.Id } },
                                },
                        },
                        new()
                        {
                            ConditionRules = new List<ConditionRule>
                            {
                                new() { Type = EnumSubjectConditionRuleType.Age, FromAge = 14, ToAge = 16, OperatorType = EnumOperatorType.Between },
                                new() { Type = EnumSubjectConditionRuleType.CurrentLevel, LevelIds = new[] { a1.Id }, OperatorType = EnumOperatorType.Include }
                            },
                            ConditionValues =
                                new List<ConditionValue>
                                {
                                    new() { Type = EnumSubjectConditionValueType.Recommended, LevelIds = new[] { a1.Id } },
                                    new() { Type = EnumSubjectConditionValueType.Advanced, LevelIds = new[] { a2.Id } },
                                },
                        },
                        new()
                        {
                            ConditionRules = new List<ConditionRule>
                            {
                                new() { Type = EnumSubjectConditionRuleType.Age, FromAge = 14, ToAge = null, OperatorType = EnumOperatorType.LessThan },
                                new() { Type = EnumSubjectConditionRuleType.CurrentLevel, LevelIds = new[] { a1.Id }, OperatorType = EnumOperatorType.Include }
                            },
                            ConditionValues =
                                new List<ConditionValue>
                                {
                                    new() { Type = EnumSubjectConditionValueType.Recommended, LevelIds = new[] { a1.Id } },
                                    new() { Type = EnumSubjectConditionValueType.Advanced, LevelIds = new[] { a2.Id } },
                                },
                        },
                        new()
                        {
                            ConditionRules = new List<ConditionRule>
                            {
                                new() { Type = EnumSubjectConditionRuleType.Age, FromAge = 14, ToAge = null, OperatorType = EnumOperatorType.LessThan },
                                new() { Type = EnumSubjectConditionRuleType.CurrentLevel, LevelIds = new[] { b2.Id }, OperatorType = EnumOperatorType.Include }
                            },
                            ConditionValues =
                                new List<ConditionValue>
                                {
                                    new() { Type = EnumSubjectConditionValueType.Basic, LevelIds = new[] { b1.Id } },
                                    new() { Type = EnumSubjectConditionValueType.Recommended, LevelIds = new[] { b2.Id } },
                                    new() { Type = EnumSubjectConditionValueType.Advanced, LevelIds = new[] { c1.Id } },
                                },
                        },
                        new()
                        {
                            ConditionRules = new List<ConditionRule>
                            {
                                new() { Type = EnumSubjectConditionRuleType.Age, FromAge = 16, ToAge = null, OperatorType = EnumOperatorType.GreaterThanEqual },
                                new() { Type = EnumSubjectConditionRuleType.CurrentLevel, LevelIds = new[] { a1.Id }, OperatorType = EnumOperatorType.Include }
                            },
                            ConditionValues =
                                new List<ConditionValue>
                                {
                                    new() { Type = EnumSubjectConditionValueType.Recommended, LevelIds = new[] { a1.Id, fA1.Id } },
                                    new() { Type = EnumSubjectConditionValueType.Advanced, LevelIds = new[] { a2.Id, fA2.Id } },
                                },
                        },
                        new()
                        {
                            ConditionRules = new List<ConditionRule>
                            {
                                new() { Type = EnumSubjectConditionRuleType.Age, FromAge = 16, ToAge = null, OperatorType = EnumOperatorType.GreaterThanEqual },
                                new() { Type = EnumSubjectConditionRuleType.CurrentLevel, LevelIds = new[] { a1.Id }, OperatorType = EnumOperatorType.Include }
                            },
                            ConditionValues =
                                new List<ConditionValue>
                                {
                                    new() { Type = EnumSubjectConditionValueType.Recommended, LevelIds = new[] { a1.Id, fA1.Id } },
                                    new() { Type = EnumSubjectConditionValueType.Advanced, LevelIds = new[] { a2.Id, fA2.Id } },
                                },
                        },
                    }
            },
        };

        public static List<Category> subjects = new() { englishSubject, japanSubject };
    }
}
