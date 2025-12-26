// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.EntityModels;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace Fsel.Course.Infrastructure.Repositories
{
    public class ExtraPracticeRepository : BaseRepository<ExtraPractice>, IExtraPracticeRepository
    {
        public ExtraPracticeRepository(CourseDbContext dbContext, CourseReadDbContext readDbContext, AuthContext authContext, IMapper mapper)
            : base(dbContext, readDbContext, authContext, mapper)
        {
        }

        public async Task<ExtraPracticeModel?> GetIncludeAllAsync(Guid? id)
        {
            return await Queryable.Include(x => x.ExtraPracticeChapters.Where(n => !n.IsDeleted))
                                    .Where(x => x.Id == id).Select(x => new ExtraPracticeModel
                                    {
                                        Id = x.Id,
                                        Abstract = x.Abstract,
                                        Author = x.Author,
                                        Code = x.Code,
                                        CourseLevel = x.CourseLevel,
                                        BookBackgroundPath = x.BookBackgroundPath,
                                        BookCoverPath = x.BookCoverPath,
                                        BookFilePath = x.BookFilePath,
                                        InstructionContent = x.InstructionContent,
                                        IsActive = x.IsActive,
                                        ImagePath = x.ImagePath,
                                        Name = x.Name,
                                        Type = x.Type,
                                        VideoId = x.VideoId,
                                        VideoLink = x.VideoLink,
                                        MockTestId = x.MockTestId,
                                        PlacementTestId = x.PlacementTestId,
                                        ExtraPracticeChapters = x.ExtraPracticeChapters.Where(n => !n.IsDeleted).OrderBy(x => x!.PageNumber).Select(x => new ExtraPracticeChapterModel
                                        {
                                            Id = x.Id,
                                            Description = x.Description,
                                            Name = x.Name,
                                            ExtraPracticeId = x.ExtraPracticeId,
                                            PageNumber = x.PageNumber,
                                            Exercises = x.ExtraPracticeExercises.OrderBy(x => x!.CreatedDate).Select(x => x.Exercise).Select(x => new ExerciseModel
                                            {
                                                Id = x!.Id,
                                                MediaPost = x.MediaPost,
                                                Name = x.Name,
                                                CourseSkill = x.CourseSkill,
                                                SkillId = x.SkillId,
                                                SkillName = x.Skill != null ? x.Skill.Name : null,
                                                Questions = x.ExerciseQuestions.Select(x => x.Question).OrderBy(x => x!.CreatedDate).Select(m => new QuestionModel()
                                                {
                                                    Id = m!.Id,
                                                    QuestionType = m!.QuestionType,
                                                    CorrectTotal = m!.CorrectTotal,
                                                    Explanation = m!.Explanation,
                                                    Ungraded = m!.Ungraded,
                                                    Config = m.Config,
                                                }).ToList(),
                                            }).ToList(),
                                        }).ToList(),
                                        Video = x.Video == null ? null : new VideoModel
                                        {
                                            Id = x.Video.Id,
                                            Name = x.Video.Name,
                                            VideoFilePath = x.Video.VideoFilePath,
                                            IsActive = x.Video.LessonVideos.Any(),
                                            TeacherId = x.Video.TeacherId,
                                            CourseLevel = x.Video.CourseLevel,
                                            VideoTimeCodes = x.Video.VideoTimeCodes.Where(x => !x.IsDeleted).OrderBy(x => x!.CreatedDate).Select(x => new VideoTimeCodeModel
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
                                        },
                                        Exercises = x.ExtraPracticeExercises.Where(n => n.Exercise != null && !n.IsDeleted).Select(n => n.Exercise).OrderBy(x => x!.CreatedDate).Select(n => new ExerciseModel
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
                                    }).FirstOrDefaultAsync();
        }

        public override async Task<ExtraPractice?> GetIncludeByIdAsync(Guid id)
        {
            try
            {
                return await Queryable.Include(x => x.ExtraPracticeChapters.Where(n => !n.IsDeleted).OrderBy(x => x.PageNumber))
                                    .ThenInclude(x => x.ExtraPracticeExercises.Where(n => !n.IsDeleted).OrderBy(x => x.CreatedDate))
                                    .ThenInclude(x => x.Exercise)
                                    .ThenInclude(x => x!.ExerciseQuestions.Where(n => !n.IsDeleted).OrderBy(x => x.CreatedDate))
                                    .ThenInclude(x => x.Question)
                                    .Include(x => x.LessonExtraPractices.Where(n => !n.IsDeleted).OrderBy(x => x.CreatedDate))
                                    .Include(x => x.Video)
                                    .ThenInclude(x => x!.VideoTimeCodes.Where(x => !x.IsDeleted).OrderBy(x => x.CreatedDate))
                                    .ThenInclude(x => x.TimeCodeExercises.Where(x => !x.IsDeleted && x.Exercise != null).OrderBy(x => x.CreatedDate))
                                    .ThenInclude(x => x.Exercise)
                                    .ThenInclude(x => x!.ExerciseQuestions.Where(x => !x.IsDeleted).OrderBy(x => x.CreatedDate))
                                    .ThenInclude(x => x.Question)
                                    .Include(x => x.ExtraPracticeExercises.OrderBy(x => x.CreatedDate))
                                    .ThenInclude(x => x.Exercise)
                                    .ThenInclude(x => x!.ExerciseQuestions.Where(x => !x.IsDeleted).OrderBy(x => x.CreatedDate))
                                    .ThenInclude(x => x.Question)
                                    .FirstOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ExtraPractice?> GetIncludeSkillReadingAsync(Guid? id)
        {
            try
            {
                return await Queryable.Include(x => x.MockTest)
                                    .ThenInclude(x => x!.MockTestSections)
                                    .ThenInclude(x => x.SectionGroup)
                                    .ThenInclude(x => x!.Sections)
                                    .ThenInclude(x => x.SectionParts)
                                    .ThenInclude(x => x.SectionQuestions)
                                    .ThenInclude(x => x.Question)
                                    .Include(x => x.MockTest)
                                    .ThenInclude(x => x!.MockTestSections)
                                    .ThenInclude(x => x.SectionGroup)
                                    .ThenInclude(x => x.Skill)
                                    .FirstOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ExtraPractice?> GetIncludeByTypeVideoEmbedAsync(Guid? id)
        {
            try
            {
                return await Queryable.Include(x => x.ExtraPracticeExercises)
                                .ThenInclude(x => x.Exercise)
                                .ThenInclude(x => x!.ExerciseQuestions)
                                .ThenInclude(x => x.Question).Include(x => x.ExtraPracticeExercises)
                                .ThenInclude(x => x.Exercise)
                                .ThenInclude(x => x.Skill)
                                .FirstOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ExtraPractice?> GetIncludeByTypeInteractiveVideoAsync(Guid? id)
        {
            try
            {
                return await Queryable.Include(x => x.Video)
                                .ThenInclude(x => x!.VideoTimeCodes)
                                .ThenInclude(x => x.TimeCodeExercises)
                                .ThenInclude(x => x.Exercise)
                                .ThenInclude(x => x!.ExerciseQuestions)
                                .ThenInclude(x => x.Question)
                                .Include(x => x.Video)
                                .ThenInclude(x => x!.VideoTimeCodes)
                                .ThenInclude(x => x.TimeCodeExercises)
                                .ThenInclude(x => x.Exercise).ThenInclude(x => x.Skill)
                                .FirstOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ExtraPractice?> GetIncludeByTypeBookAsync(Guid? id)
        {
            try
            {
                return await Queryable.Include(x => x.ExtraPracticeChapters)
                                .ThenInclude(x => x.ExtraPracticeExercises)
                                .ThenInclude(x => x.Exercise)
                                .ThenInclude(x => x!.ExerciseQuestions)
                                .ThenInclude(x => x.Question)
                                .Include(x => x.ExtraPracticeChapters)
                                .ThenInclude(x => x.ExtraPracticeExercises)
                                .ThenInclude(x => x.Exercise)
                                .ThenInclude(x => x.Skill)
                                .FirstOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ExtraPractice?> GetIncludeByPlacementTestAsync(Guid? id)
        {
            try
            {
                return await Queryable.Include(x => x.PlacementTest)
                                .ThenInclude(x => x!.PlacementTestSections)
                                .ThenInclude(x => x.SectionGroup)
                                .ThenInclude(x => x!.Sections)
                                .ThenInclude(x => x.SectionQuestions)
                                .ThenInclude(x => x.Question)
                                .Include(x => x.PlacementTest)
                                .ThenInclude(x => x!.PlacementTestSections)
                                .ThenInclude(x => x.SectionGroup)
                                .ThenInclude(x => x.Skill)
                                .FirstOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
