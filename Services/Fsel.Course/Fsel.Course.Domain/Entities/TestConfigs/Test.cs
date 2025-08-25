// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.TestConfigs
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Attributes;
    using Fsel.Common.Enums;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;

    public class Test : Entity, IVersionEntity
    {
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [RegexValid(Regex = @"^[a-zA-Z0-9_ ]{1,150}$", ErrorMessage = nameof(EnumSystemErrorCode.InValidFormat))]
        [MaxLength(150, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Name { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [RegexValid(Regex = @"^[a-zA-Z0-9_]{1,150}$", ErrorMessage = nameof(EnumSystemErrorCode.InValidFormat))]
        [MaxLength(150, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Code { get; set; }

        public bool IsArchive { get; set; }
        public Guid OriginalId { get; set; }
        public int Version { get; set; }
        public EnumVersionStatus VersionStatus { get; set; }
        public EnumScoringFormulaType ScoringFormulaType { get; set; }

        public Guid ProgramId { get; set; }
        public Category? Program { get; set; }
        public Guid LevelId { get; set; }
        public Level? Level { get; set; }

        public ICollection<TestSection> TestSections { get; set; } = new List<TestSection>();
        public ICollection<CategoryTestBank> CategoryTestBanks { get; set; } = new List<CategoryTestBank>();

        public override async Task<bool> IsValid(IServiceProvider serviceProvider)
        {
            await base.IsValid(serviceProvider);
            if (VersionStatus == EnumVersionStatus.OldVersion)
            {
                AddErrorResults(new ErrorResult
                {
                    ErrorCode = nameof(EnumSystemErrorCode.InValidFormat),
                    Errors =
                        {
                            new Error
                            {
                                FieldName =nameof(VersionStatus),
                            }
                        }
                });
            }

            TestSections.ForEach(testSection =>
            {
                ValidateTestSectionRecursively(testSection);
                ValidateTestLayout(testSection);
            });

            return !ErrorMessages.Any();
        }

        private void ValidateTestLayout(TestSection testSection)
        {
            if (testSection.LayoutType.HasValue)
            {
                var isLayOutBasicError = testSection.LayoutType == EnumTestLayoutType.Basic && testSection.TestAISettings.Any();
                var isLayOutError = testSection.LayoutType != EnumTestLayoutType.Basic && testSection.TestSectionQuestions.Any();
                if (isLayOutError || isLayOutBasicError)
                {
                    AddErrorResults(new ErrorResult
                    {
                        ErrorCode = nameof(EnumSystemErrorCode.InValidFormat),
                        Errors =
                        {
                            new Error
                            {
                                FieldName =nameof(testSection.LayoutType),
                            }
                        }
                    });
                }
            }
        }

        private void ValidateTestSectionRecursively(TestSection section)
        {
            if (!section.IsValid())
            {
                AddErrorResults(section.ErrorMessages);
            }
            ValidateTestAISettings(section.TestAISettings);
            ValidateTestSectionQuestions(section.TestSectionQuestions);
            section.TestSections.ForEach(childSection =>
            {
                ValidateTestSectionRecursively(childSection);
            });
        }

        private void ValidateTestAISettings(IEnumerable<TestAISetting> aiSettings)
        {
            aiSettings.ForEach(aiSetting =>
            {
                if (!aiSetting.IsValid())
                {
                    AddErrorResults(aiSetting.ErrorMessages);
                }
                ValidateTestAICriteriaSettings(aiSetting.TestAICriteriaSettings);
            });
        }

        private void ValidateTestAICriteriaSettings(IEnumerable<TestAICriteriaSetting> testAICriteriaSettings)
        {
            testAICriteriaSettings.ForEach(criteria =>
            {
                if (!criteria.IsValid())
                {
                    AddErrorResults(criteria.ErrorMessages);
                }
            });
        }

        private void ValidateTestSectionQuestions(IEnumerable<TestSectionQuestion> sectionQuestions)
        {
            sectionQuestions.ForEach(questionLink =>
            {
                if (!questionLink.IsValid())
                {
                    AddErrorResults(questionLink.ErrorMessages);
                }
                if (questionLink.Question != null && !questionLink.Question.IsValid())
                {
                    AddErrorResults(questionLink.Question.ErrorMessages);
                }
            });
        }

        public async Task<bool> ValidateDuplicateTest(ITestRepository testRepository)
        {
            var isDuplicatedTest = await testRepository.Queryable
                .AnyAsync(u => u.Code == Code && u.OriginalId != OriginalId)
                .ConfigureAwait(false);
            if (isDuplicatedTest)
            {
                AddErrorResults(new ErrorResult
                {
                    ErrorCode = nameof(EnumSystemErrorCode.DataAlreadyExist),
                    Errors = { new Error
                    {
                        FieldName = $"{nameof(Code)} and {nameof(OriginalId)}",
                    } }
                });
            }
            return isDuplicatedTest;
        }

        public async Task<bool> ValidateProgram(ICategoryRepository categoryRepository)
        {
            var exists = await categoryRepository.Queryable
                       .AnyAsync(u => u.Id == ProgramId)
                       .ConfigureAwait(false);

            if (!exists)
            {
                AddErrorResults(new ErrorResult
                {
                    ErrorCode = nameof(EnumSystemErrorCode.DataNotExist),
                    Errors = { new Error
                    {
                        FieldName = nameof(ProgramId)
                    } }
                });
            }
            return exists;
        }

        public async Task<bool> ValidateLevel(ILevelRepository levelRepository)
        {
            var exists = await levelRepository.Queryable
                       .AnyAsync(u => u.Id == LevelId)
                       .ConfigureAwait(false);

            if (!exists)
            {
                AddErrorResults(new ErrorResult
                {
                    ErrorCode = nameof(EnumSystemErrorCode.DataNotExist),
                    Errors = { new Error
                    {
                        FieldName = nameof(LevelId)
                    } }
                });
            }
            return exists;
        }

        public async Task<bool> ValidateScoringFormula()
        {
            if (ScoringFormulaType == EnumScoringFormulaType.BandScore)
            {
                double totalPercent = 0;
                TestSections.ForEach(p =>
                {
                    if (p.Percent.HasValue)
                    {
                        totalPercent += p.Percent.Value;
                    }
                });
                if (totalPercent <= 99 || totalPercent > 100)
                {
                    AddErrorResults(new ErrorResult
                    {
                        ErrorCode = nameof(EnumSystemErrorCode.Min),
                        Errors =
                        {
                            new Error
                                {
                                    FieldName = nameof(totalPercent),
                                }
                        },
                    });
                    return false;
                }
            }
            else
            {
                double totalPercent = 0;
                TestSections.ForEach(p =>
                {
                    if (p.Percent.HasValue)
                    {
                        totalPercent += p.Percent.Value;
                    }
                });
                if (totalPercent <= 99 || totalPercent > 100)
                {
                    AddErrorResults(new ErrorResult
                    {
                        ErrorCode = nameof(EnumSystemErrorCode.Min),
                        Errors =
                        {
                            new Error
                                {
                                    FieldName = nameof(totalPercent),
                                }
                        },
                    });
                }
            }
            return true;
        }
    }
}
