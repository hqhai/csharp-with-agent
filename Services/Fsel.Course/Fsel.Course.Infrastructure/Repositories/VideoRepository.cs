// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Core.Base;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Shared.Enums;
using Fsel.Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Fsel.Course.Infrastructure.Repositories
{
    public class VideoRepository : BaseRepository<Video>, IVideoRepository
    {
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IMapper _mapper;
        private readonly ILessonRepository _lessonRepository;
        private readonly ILogger<VideoRepository> _logger;
        private readonly ILessonModuleRepository _lessonModuleRepository;

        public VideoRepository(CourseDbContext dbContext,
            CourseReadDbContext courseReadDbContext,
            AuthContext authContext,
            ILessonResultRepository lessonResultRepository,
            AutoMapper.IMapper mapper,
            ILessonRepository lessonRepository,
            ILogger<VideoRepository> logger,
            ILessonModuleRepository lessonModuleRepository) : base(dbContext, courseReadDbContext, authContext, mapper)
        {
            _lessonResultRepository = lessonResultRepository;
            _mapper = mapper;
            _lessonRepository = lessonRepository;
            _logger = logger;
            _lessonModuleRepository = lessonModuleRepository;
        }

        public async Task<bool> IsVideoUsed(Guid? id)
        {
            return await (from baseQ in Queryable
                          join lessonModule in _lessonModuleRepository.Queryable on baseQ.OriginalId equals lessonModule.OriginalId
                          where baseQ.Id == id
                          select baseQ.Id).AnyAsync();
        }

        public override async Task<Video?> GetIncludeByIdAsync(Guid id)
        {
            try
            {
                return await Queryable
                .Include(x => x.VideoTimeCodes.Where(n => !n.IsDeleted))
                .ThenInclude(x => x.TimeCodeExercises.Where(n => !n.IsDeleted))
                .ThenInclude(x => x.Exercise)
                .ThenInclude(x => x!.ExerciseQuestions.Where(n => !n.IsDeleted))
                .ThenInclude(x => x.Question)
                .FirstOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<double> GetPercent(Guid courseId, Guid unitId, Guid? studentId)
        {
            var lessonResults = await _lessonResultRepository.Queryable.Include(x => x.Lesson).Include(x => x.VideoResult).Where(x => x.CourseId == courseId && x.UnitId == unitId && x.StudentId == studentId).ToListAsync();

            var lessonIds = lessonResults.Select(x => x.Lesson!.Id).ToList();
            var videoResultIds = lessonResults.Where(x => x.VideoResult != null).Select(x => x.VideoResult!.Id).ToList();

            var videoIds = await _lessonRepository.Queryable.Include(x => x.LessonVideos)
                                                .WhereBulkContains(lessonIds, x => x.Id)
                                                .SelectMany(x => x.LessonVideos)
                                                .Select(x => x.VideoId)
                                                .ToListAsync();

            var videos = await Queryable.Include(x => x.VideoTimeCodes)
                                    .ThenInclude(x => x.VideoTimeCodeResults.Where(x => videoResultIds.Contains(x.VideoResultId)))
                                    .Where(x => videoIds.Contains(x.Id))
                                    .ToListAsync();

            var videoTimeCodes = videos.SelectMany(x => x.VideoTimeCodes).Where(x => x.TimeCodeType == EnumTimeCodeType.UnitTest).ToList();
            var videoTimeCodeResults = videoTimeCodes.SelectMany(x => x.VideoTimeCodeResults).Where(x => videoResultIds.Contains(x.VideoResultId)).Where(x => x.Status == EnumResultStatus.Done).ToList();
            return NumberHelper.GetPercent(videoTimeCodeResults.Count, videoTimeCodes.Count);
        }

        public async Task<VideoModel?> GetIncludeAllAsync(Guid? id)
        {
            return await Queryable
                            .Include(i => i.VideoTimeCodes.Where(x => !x.IsDeleted))
                            .ThenInclude(x => x.TimeCodeExercises.Where(x => !x.IsDeleted && x.Exercise != null))
                            .ThenInclude(x => x.Exercise)
                            .ThenInclude(x => x!.ExerciseQuestions.Where(x => !x.IsDeleted))
                            .ThenInclude(x => x.Question)
                            .Where(x => x.Id == id)
                            .Select(i => new VideoModel
                            {
                                Id = i.Id,
                                Name = i.Name,
                                VideoFilePath = i.VideoFilePath,
                                TeacherId = i.TeacherId,
                                SubFilePath = i.SubFilePath,
                                Type = i.Type,
                                CourseLevel = i.CourseLevel,
                                VersionStatus = i.VersionStatus,
                                Version = i.Version,
                                Program = _mapper.Map<ProgramModel>(i.Program),
                                Level = _mapper.Map<LevelModel>(i.Level),
                                IsUseStudent = i.VideoResults.Any(),
                                VideoTimeCodes = i.VideoTimeCodes.Where(x => !x.IsDeleted).OrderBy(x => x!.DisplayTime).Select(x => new VideoTimeCodeModel
                                {
                                    Id = x.Id,
                                    DisplayTime = x.DisplayTime,
                                    ExecutionTime = x.ExecutionTime,
                                    TimeCodeType = x.TimeCodeType,
                                    VideoId = x.VideoId,
                                    Exercises = x.TimeCodeExercises.Where(n => n.Exercise != null && !n.IsDeleted).Select(n => n.Exercise).OrderBy(x => x!.CreatedDate).Select(n => new ExerciseModel
                                    {
                                        Id = n!.Id,
                                        Name = n.Name,
                                        MediaPost = n.MediaPost,
                                        CourseSkill = n.CourseSkill,
                                        SkillId = n.SkillId,
                                        SkillName = n.Skill != null ? n.Skill.Name : null,
                                        Questions = n.ExerciseQuestions.Where(m => m.Question != null && !m.IsDeleted).Select(m => m.Question).OrderBy(x => x!.CreatedDate).Select(m => new QuestionModel()
                                        {
                                            Id = m!.Id,
                                            QuestionType = m.QuestionType,
                                            Explanation = m.Explanation,
                                            Ungraded = m.Ungraded,
                                            CorrectTotal = m.CorrectTotal,
                                            Config = m.Config
                                        }).ToList()
                                    }).ToList(),
                                }).ToList(),
                            }).FirstOrDefaultAsync();
        }

        public IQueryable<VideoSearchModel> SearchAsync(EnumTimeCodeType? codeType, Guid? teacherId, EnumCourseLevel? courseLevel)
        {
            try
            {
                return Queryable.Where(p => !p.IsArchive).Include(x => x.LessonVideos.Where(y => !y.IsDeleted))
                                    .Include(video => video.VideoTimeCodes.Where(x => !x.IsDeleted))
                                    .ThenInclude(videoTimeCode => videoTimeCode.TimeCodeExercises.Where(x => !x.IsDeleted && x.Exercise != null))
                                    .ThenInclude(timeCodeExercise => timeCodeExercise.Exercise)
                                    .Where(x => x.Type == EnumVideoType.Lesson)
                                    .Where(x => !courseLevel.HasValue || x.CourseLevel == courseLevel.Value)
                                    .Where(x => !teacherId.HasValue || x.TeacherId == teacherId.Value)
                                    .Where(x => !codeType.HasValue || x.VideoTimeCodes.Select(n => n.TimeCodeType).Contains(codeType.Value))
                                    .OrderBy(x => x!.CreatedDate)
                                    .Select(video => new VideoSearchModel
                                    {
                                        Id = video.Id,
                                        Name = video.Name,
                                        IsActive = video.LessonVideos.Any(),
                                        VideoFilePath = video.VideoFilePath,
                                        CourseLevel = video.CourseLevel,
                                        CreatedDate = video.CreatedDate,
                                        CreatedFullName = video.CreatedFullName,
                                        UpdatedDate = video.UpdatedDate,
                                        UpdatedFullName = video.UpdatedFullName,
                                        Exercises = video.VideoTimeCodes
                                                    .SelectMany(videoTimeCode => videoTimeCode.TimeCodeExercises.Where(y => !y.IsDeleted && y.Exercise != null))
                                                    .Select(timeCodeExercise => timeCodeExercise.Exercise)
                                                    .GroupBy(excercise => excercise!.CourseSkill)
                                                    .OrderByDescending(courseSkillGroup => courseSkillGroup.Count())
                                                    .Select(courseSkillGroup => new VideoExerciseSearchModel
                                                    {
                                                        CourseSkill = courseSkillGroup.Key,
                                                        Count = courseSkillGroup.Count()
                                                    }).ToList()
                                    });
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
