// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Test
{
    using Domain.Entities;
    using Domain.Entities.SubjectConditionRuleConfigs;
    using Domain.Enums;
    using Shared.Enums;

    public static class Data
    {
        public static Level preA1 = new Level() { Id = new Guid("D3C8F2E3-1E2C-4A5C-8E3C-3F2D8F2E3A1B"), Name = "Pre A1", };
        public static Level a1 = new Level { Id = new Guid("5A1F23AD-57AC-410E-8E85-9056CC3B3845"), Name = "A1", LevelOrder = 0 };
        public static Level a2 = new Level { Id = new Guid("05AE8897-88A5-446E-B161-D939F54E978B"), Name = "A2", LevelOrder = 1 };
        public static Level b1 = new Level { Id = new Guid("CF8FF4A4-7314-4CBE-9091-5F6F5D8EB098"), Name = "B1", LevelOrder = 2 };
        public static Level b1Plus = new Level { Id = new Guid("CF8FF4A4-7314-4CBE-9091-5F6F5D8EB098"), Name = "B1 Plus", LevelOrder = 3 };
        public static Level b2 = new Level { Id = new Guid("AE40457A-93C7-4C16-9E61-6BADAE50DC60"), Name = "B2", LevelOrder = 4 };
        public static Level c1 = new Level { Id = new Guid("AE40457A-93C7-4C16-9E61-6BADAE50DC61"), Name = "C1", LevelOrder = 5 };

        public static Level fA1 = new Level { Id = new Guid("318947B4-6478-4608-AE30-1A7CBC9930E1"), Name = "FA1", LevelOrder = 0 };
        public static Level fA2 = new Level { Id = new Guid("CD72DBE7-A94F-41CD-87B1-A2CE0844BA28"), Name = "FA2", LevelOrder = 1 };
        public static Level fB1 = new Level { Id = new Guid("6CE7E985-B5D0-4336-9A56-99CF3DE86E75"), Name = "FB1", LevelOrder = 2 };

        public static Level m1 = new Level { Id = new Guid("703C2231-A1DE-4633-954F-BC06AF38A673"), Name = "IELTS - Mindset 1 -  5.", LevelOrder = 0 };
        public static Level m2 = new Level { Id = new Guid("9756DC91-FF84-49F0-9428-4840456E819A"), Name = "IELTS - Mindset 2 -  6.", LevelOrder = 1 };
        public static Level m3 = new Level { Id = new Guid("919C82E7-ABCA-4CE1-AC01-374F0F4FDD63"), Name = "IELTS - Mindset 3 -  7.", LevelOrder = 2 };

        public static Level n5 = new Level { Id = new Guid("295D24C9-E2C0-407F-A6BB-533280640D32"), Name = "N5", LevelOrder = 0 };
        public static Level n4 = new Level { Id = new Guid("E6B2464F-110F-49AB-B6DC-B617A42EA015"), Name = "N4", LevelOrder = 1 };
        public static Level n3 = new Level { Id = new Guid("3833FF46-3CCF-40E5-8A13-ADA016087B11"), Name = "N3", LevelOrder = 2 };

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


        public static Category englishSubject = new()
        {
            Id = new Guid("6DDD02D5-0D95-4B2E-82BA-BAB84E7D3CBF"),
            Name = "English",
            Type = EnumTypeCategory.Subject,
            Categorys = new List<Category>
            {
                // project
                new()
                {
                    Id = new Guid("F260A950-5815-4FBE-95B3-9C4D05353439"),
                    ParentId = new Guid("6DDD02D5-0D95-4B2E-82BA-BAB84E7D3CBF"),
                    Name = "English",
                    Type = EnumTypeCategory.Subject,
                    Categorys = new List<Category>
                    {
                        new()
                        {
                            Id = new Guid("41A8C234-013B-4125-9586-DF0CBD3F19FB"),
                            ParentId = new Guid("F260A950-5815-4FBE-95B3-9C4D05353439"),
                            Name = "Foundation English",
                            Type = EnumTypeCategory.Program,
                            Levels =
                                new List<Level> { fA1, fA2, fB1 },
                            TestMode = EnumTestMode.Default
                        },
                        new()
                        {
                            Id = new Guid("7B0678CC-8969-423A-B604-AC9B65ED61FA"),
                            ParentId = new Guid("F260A950-5815-4FBE-95B3-9C4D05353439"),
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
                            Id = new Guid("3FD08074-A9EF-41D2-8D02-53A31AD16E7A"),
                            ParentId = new Guid("F260A950-5815-4FBE-95B3-9C4D05353439"),
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
            Id = new Guid("EDF59A5D-64B2-4625-831D-6941423AD414"),
            Name = "SubjectJPN",
            Type = EnumTypeCategory.Subject,
            Categorys = new List<Category>
            {
                // project
                new()
                {
                    Id = new Guid("E260603B-2465-453E-803C-A543BCFA4C29"),
                    ParentId = new Guid("EDF59A5D-64B2-4625-831D-6941423AD414"),
                    Name = "Japanese",
                    Type = EnumTypeCategory.Subject,
                    Categorys = new List<Category>
                    {
                        new()
                        {
                            Id = new Guid("A651E44F-7509-40C4-AD53-D03C3288CCF4"),
                            ParentId = new Guid("E260603B-2465-453E-803C-A543BCFA4C29"),
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


        public static List<Category> subjects = new() { englishSubject, japanSubject };
    }
}
