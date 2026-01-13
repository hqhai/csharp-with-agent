// Copyright (c) Atlantic. All rights reserved.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Fsel.Common.ActionResults;
using Fsel.Common.Attributes;
using Fsel.Common.Enums;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Common.Helpers;
using Fsel.Core.Entities;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.Enums.ErrorCodes;
using Fsel.Course.Domain.IRepositories;
using Fsel.Shared.Enums;
using Fsel.Shared.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Domain.Entities
{
    public class Video : Entity, IVersionEntity
    {
        /// <summary>
        /// Tên Video
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(200, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        [RegexValid(Regex = @"^[^<>]*$", ErrorMessage = nameof(EnumVideoErrorCode.InvalidKeywordCharacter))]
        public string? Name { get; set; }

        private string? _videoFilePath;

        /// <summary>
        /// Link Video
        /// </summary>
        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? VideoFilePath
        {
            get { return _videoFilePath; }
            set { _videoFilePath = value; TimeCount = MediaHelper.GetMediaDurationAsync(value.AddS3BaseUrl()); }
        }

        private int? _timeCount;

        public int? TimeCount
        {
            get
            {
                _timeCount ??= MediaHelper.GetMediaDurationAsync(VideoFilePath.AddS3BaseUrl());
                return _timeCount;
            }
            set { _timeCount = value; }
        }

        /// <summary>
        /// Sub File Path
        /// </summary>
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? SubFilePath { get; set; }

        /// <summary>
        /// Loại Video
        /// </summary>
        public EnumVideoType Type { get; set; }

        /// <summary>
        /// Giáo Viên ID
        /// </summary>
        public Guid TeacherId { get; set; }

        /// <summary>
        /// Trình độ khóa
        /// </summary>
        public EnumCourseLevel CourseLevel { get; set; }

        /// <summary>
        /// Trạng thái Archive
        /// </summary>
        public bool IsArchive { get; set; }

        public EnumVersionStatus VersionStatus { get; set; }
        public int Version { get; set; }
        public Guid OriginalId { get; set; }

        public Guid? LevelId { get; set; }
        public Level? Level { get; set; }

        public Guid? ProgramId { get; set; }
        public Category? Program { get; set; }

        public ExtraPractice? ExtraPractice { get; set; }
        public EnumVersion VersionType { get; set; } = EnumVersion.V2;

        public string? VideoPercentConfigStr { get; set; }

        [NotMapped]
        public IList<VideoPercentConfig>? VideoPercentConfigs
        {
            get { return ConvertHelper.Deserialize<IList<VideoPercentConfig>>(VideoPercentConfigStr); }
            set
            {
                if (value != null)
                {
                    VideoPercentConfigStr = ConvertHelper.Serialize(value);
                }
            }
        }

        public ICollection<LessonVideo> LessonVideos { get; set; } = new List<LessonVideo>();
        public ICollection<VideoTimeCode> VideoTimeCodes { get; set; } = new List<VideoTimeCode>();
        public ICollection<VideoResult> VideoResults { get; set; } = new List<VideoResult>();
        public ICollection<VideoSubFilePath> VideoSubFilePaths { get; set; } = new List<VideoSubFilePath>();

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

        public bool ValidateVideoPercentConfigs()
        {
            #region Validate ConfigVideos

            if (VideoPercentConfigs == null || !VideoPercentConfigs.Any())
            {
                AddErrorResults(new ErrorResult
                {
                    ErrorCode = nameof(EnumSystemErrorCode.DataNotExist),
                    Errors =
            {
                new Error
                {
                    FieldName = nameof(VideoPercentConfigs)
                }
            }
                });

                return false;
            }

            // Không cho phép trùng Type
            var duplicatedTypes = VideoPercentConfigs
                .GroupBy(x => x.Type)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicatedTypes.Any())
            {
                AddErrorResults(new ErrorResult
                {
                    ErrorCode = nameof(EnumSystemErrorCode.DataAlreadyExist),
                    Errors =
                    {
                        new Error
                        {
                            FieldName = nameof(VideoPercentConfigs),
                            ErrorValues = new List<object>
                            {
                                $"Duplicate type(s): {string.Join(", ", duplicatedTypes)}"
                            }
                        }
                    }
                });

                return false;
            }

            // Percent phải >= 0
            if (VideoPercentConfigs.Any(x => x.Percent < 0))
            {
                AddErrorResults(new ErrorResult
                {
                    ErrorCode = nameof(EnumSystemErrorCode.InValidFormat),
                    Errors =
                    {
                        new Error
                        {
                            FieldName = nameof(VideoPercentConfigs),
                            ErrorValues = new List<object>
                            {
                                "Percent must be greater than or equal to 0"
                            }
                        }
                    }
                });

                return false;
            }

            // Tổng Percent phải = 100
            var totalPercent = VideoPercentConfigs.Sum(x => x.Percent);
            if (Math.Abs(totalPercent - 100.0) > 0.0001)
            {
                AddErrorResults(new ErrorResult
                {
                    ErrorCode = nameof(EnumSystemErrorCode.InValidFormat),
                    Errors =
                    {
                        new Error
                        {
                            FieldName = nameof(VideoPercentConfigs),
                            ErrorValues = new List<object>
                            {
                                $"Total percent must equal 100. Current total: {totalPercent}"
                            }
                        }
                    }
                });

                return false;
            }

            return true;

            #endregion Validate ConfigVideos
        }

        public async Task<bool> ValidateDuplicateVideo(IVideoRepository videoRepository)
        {
            var isDuplicatedVideo = await videoRepository.Queryable
                .AnyAsync(u => u.Name == Name && u.OriginalId != OriginalId)
                .ConfigureAwait(false);
            if (isDuplicatedVideo)
            {
                AddErrorResults(new ErrorResult
                {
                    ErrorCode = nameof(EnumSystemErrorCode.DataAlreadyExist),
                    Errors = { new Error
                    {
                        FieldName = $"{nameof(Name)} and {nameof(OriginalId)}",
                    } }
                });
            }
            return isDuplicatedVideo;
        }
    }

    public class VideoPercentConfig
    {
        public EnumTimeCodeType Type { get; set; }
        public double Percent { get; set; }
    }
}
