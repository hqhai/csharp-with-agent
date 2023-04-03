// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.Enums;
using Fsel.Core.Base;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.EntityModels;
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
                                    CourseLevel = i.CourseLevel,
                                    VideoTimeCodes = i.VideoTimeCodes.Select(x => new VideoTimeCodeModel
                                    {
                                        Id = x.Id,
                                        DisplayTime = x.DisplayTime,
                                        ExecutionTime = x.ExecutionTime,
                                        TimeCodeType = x.TimeCodeType,
                                        VideoId = x.VideoId,
                                        Exercises = x.TimeCodeExercises.Where(n => n.Exercise != null).Select(n => n.Exercise).Select(n => new ExerciseModel
                                        {
                                            Id = n!.Id,
                                            MediaPost = n.MediaPost,
                                            CourseSkill = n.CourseSkill,
                                            Questions = n.ExerciseQuestions.Where(m => m.Question != null).Select(m => m.Question).Select(m => new QuestionModel()
                                            {
                                                Id = m!.Id,
                                                QuestionType = m.QuestionType,
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

        public IQueryable<VideoSearchModel> SearchAsync(EnumTimeCodeType? codeType, Guid? teacherId, EnumCourseLevel? level)
        {
            try
            {
                return Queryable.Include(x => x.LessonVideos.Where(y => !y.IsDeleted))
                                    .Include(video => video.VideoTimeCodes.Where(x => !x.IsDeleted))
                                    .ThenInclude(videoTimeCode => videoTimeCode.TimeCodeExercises.Where(x => !x.IsDeleted && x.Exercise != null))
                                    .ThenInclude(timeCodeExercise => timeCodeExercise.Exercise)
                                    .Where(x => x.Type == EnumVideoType.Lesson)
                                    .Where(x => !level.HasValue || x.CourseLevel == level.Value)
                                    .Where(x => !teacherId.HasValue || x.TeacherId == teacherId.Value)
                                    .Where(x => codeType == null || x.VideoTimeCodes.Select(n => n.TimeCodeType).Contains(codeType.Value))
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
                                        Exercises = video.VideoTimeCodes.SelectMany(videoTimeCode => videoTimeCode.TimeCodeExercises)
                                                                                 .Where(timeCodeExercise => timeCodeExercise.Exercise != null)
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
