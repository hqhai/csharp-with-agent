// Copyright (c) Atlantic. All rights reserved.

using System.ComponentModel.DataAnnotations;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Core.Entities;
using Fsel.Shared.Enums;

namespace Fsel.Course.Domain.Entities
{
    using Common.ActionResults;
    using Common.Helpers;
    using Enums.ErrorCodes;
    using Fsel.Common.Enums;
    using Fsel.Course.Domain.Enums;
    using IRepositories;
    using Microsoft.EntityFrameworkCore;
    using Shared.Constants;
    using Shared.Helpers;
    using V1i1;

    public class Unit : Entity, IVersionEntity
    {
        /// <summary>
        /// Mã Unit
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Code { get; set; }

        /// <summary>
        /// Tên Unit
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Name { get; set; }

        /// <summary>
        /// Trình dộ Level
        /// </summary>
        public EnumCourseLevel CourseLevel { get; set; }

        /// <summary>
        /// Trạng thái Archive
        /// </summary>
        public bool IsArchive { get; set; }

        public int LessonCount { get; set; }

        public int TestCount { get; set; }

        public EnumVersionStatus VersionStatus { get; set; }

        public string? HighlightRange { get; set; }

        public int Version { get; set; }

        public Guid OriginalId { get; set; }

        public Guid? LevelId { get; set; }

        public Level? Level { get; set; }

        public Guid? ProgramId { get; set; }

        public Category? Program { get; set; }

        public ICollection<UnitSkillMockTest> UnitSkillMockTests { get; set; } = new List<UnitSkillMockTest>();
        public ICollection<CourseUnitMockTest> CourseUnitMockTests { get; set; } = new List<CourseUnitMockTest>();
        public ICollection<UnitLesson> UnitLessons { get; set; } = new List<UnitLesson>();
        public ICollection<UnitResult> UnitResults { get; set; } = new List<UnitResult>();
        public ICollection<UnitModule> UnitModules { get; set; } = new List<UnitModule>();
        public ICollection<LessonResult> LessonResults { get; set; } = new List<LessonResult>();
        public ICollection<MockTestResult> MockTestResults { get; set; } = new List<MockTestResult>();

        #region behaviors

        public override async Task<bool> IsValid(IServiceProvider serviceProvider)
        {
            await base.IsValid(serviceProvider);
            if (UnitModules == null || !UnitModules.Any())
            {
                AddErrorResults(new ErrorResult
                {
                    ErrorCode = nameof(EnumSystemErrorCode.DataNotExist),
                    Errors = { new Error(nameof(UnitModules)) }
                });
            }

            var highlightRanges = HighlightRange?.Deserialize<IList<HighlightRange>>();

            if (highlightRanges == null || !highlightRanges.Any())
            {
                AddErrorResults(new ErrorResult
                {
                    ErrorCode = nameof(EnumSystemErrorCode.DataNotExist),
                    Errors = { new Error(nameof(HighlightRange)) }
                });
            }
            else
            {
                if (highlightRanges.First().From != ValueSettings.HighlightRangeConstants.MinValue
                    || highlightRanges.Last().To != ValueSettings.HighlightRangeConstants.MaxValue)
                {
                    AddErrorResults(new ErrorResult { ErrorCode = nameof(EnumUnitErrorCode.HighlightRangeMissingBoundary) });
                }

                highlightRanges.ForEachWithPrevious((prev, current) =>
                {
                    if (prev != null && current.From != prev.To + 1)
                    {
                        AddErrorResults(new ErrorResult
                        {
                            ErrorCode = nameof(EnumUnitErrorCode.HighlightRangeOverlapOrGap),
                            Errors = { new Error
                            {
                                ErrorValues = new List<object> { current},
                            } }
                        });
                    }

                    if (current.To <= current.From)
                    {
                        AddErrorResults(new ErrorResult
                        {
                            ErrorCode = nameof(EnumUnitErrorCode.InvalidHighlightRange),
                            Errors = { new Error
                            {
                                ErrorValues = new List<object> { current},
                            } }
                        });
                    }

                    if (current.From < ValueSettings.HighlightRangeConstants.MinValue || current.To > ValueSettings.HighlightRangeConstants.MaxValue)
                    {
                        AddErrorResults(new ErrorResult
                        {
                            ErrorCode = nameof(EnumUnitErrorCode.HighlightRangeInvalidInnerValue),
                            Errors = { new Error
                            {
                                ErrorValues = new List<object> { current},
                            } }
                        });
                    }
                });
            }
            return !ErrorMessages.Any();
        }

        public async Task<bool> ValidateDuplicateUnit(IUnitRepository unitRepository)
        {
            var isDuplicatedUnit = await unitRepository.Queryable
                .AnyAsync(u => u.Code == Code)
                .ConfigureAwait(false);
            if (isDuplicatedUnit)
            {
                AddErrorResults(new ErrorResult
                {
                    ErrorCode = nameof(EnumSystemErrorCode.DataAlreadyExist),
                    Errors = { new Error
                    {
                        FieldName = $"{nameof(Code)}",
                    } }
                });
            }
            return isDuplicatedUnit;
        }

        #endregion behaviors
    }

    public class HighlightRange
    {
        public int From { get; set; }

        public int To { get; set; }

        public EnumSyllableMarked SyllableMarked { get; set; }
    }
}
