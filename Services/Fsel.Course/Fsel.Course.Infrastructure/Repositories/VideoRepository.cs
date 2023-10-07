// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Infrastructure.Repositories
{
    public class VideoRepository : BaseRepository<Video>, IVideoRepository
    {
        public VideoRepository(CourseDbContext dbContext, AuthContext authContext) : base(dbContext, authContext)
        {
        }

        public async Task<bool> IsVideoUsed(Guid? id)
        {
            return await Queryable
                 .Include(x => x.LessonVideos.Where(n => !n.IsDeleted))
                 .AnyAsync(x => x.Id == id && x.LessonVideos.Count > 0);
        }

        public override async Task<Video?> GetIncludeByIdAsync(Guid id, int? siteId = null)
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

        public async Task<VideoModel?> GetIncludeAllAsync(Guid? id)
        {
            try
            {
                return await Queryable.Include(x => x.LessonVideos.Where(y => !y.IsDeleted))
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
                                    IsActive = i.LessonVideos.Any(),
                                    TeacherId = i.TeacherId,
                                    SubFilePath = i.SubFilePath,
                                    Type = i.Type,
                                    CourseLevel = i.CourseLevel,
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
            catch (Exception)
            {
                throw;
            }
        }

        public IQueryable<VideoSearchModel> SearchAsync(EnumTimeCodeType? codeType, Guid? teacherId, EnumCourseLevel? courseLevel)
        {
            try
            {
                return Queryable.Include(x => x.LessonVideos.Where(y => !y.IsDeleted))
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

        public async Task<List<Video>?> GetListAsync(IList<Guid>? ids, IList<Guid>? videoResultIds)
        {
            if ((ids == null || !ids.Any()) || (videoResultIds == null || !videoResultIds.Any()))
            {
                return null;
            }
            return await Queryable.Include(x => x.VideoTimeCodes)
                                                             .ThenInclude(x => x.VideoTimeCodeResults.Where(y => videoResultIds.Contains(y.VideoResultId)))
                                                             .ThenInclude(x => x.VideoTimeCodeAnswers)
                                                             .Include(x => x.VideoTimeCodes)
                                                             .ThenInclude(x => x.TimeCodeExercises)
                                                             .ThenInclude(x => x.Exercise)
                                                             .ThenInclude(x => x!.ExerciseQuestions)
                                                             .ThenInclude(x => x.Question)
                                                             .Where(x => ids.Contains(x.Id))
                                                             .ToListAsync();
        }
    }
}
