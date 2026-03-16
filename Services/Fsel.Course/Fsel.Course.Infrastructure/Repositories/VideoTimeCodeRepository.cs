// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Core.Base;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Entities.SkillScoresConfigs;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.IRepositories;
using Fsel.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Infrastructure.Repositories
{
    public class VideoTimeCodeRepository : BaseRepository<VideoTimeCode>, IVideoTimeCodeRepository
    {
        private readonly IQuestionRepository _questionRepository;
        private readonly IExerciseQuestionRepository _exerciseQuestionRepository;
        private readonly IExerciseRepository _exerciseRepository;
        private readonly ITimeCodeExerciseRepository _timeCodeExerciseRepository;

        public VideoTimeCodeRepository(CourseDbContext dbContext,
            CourseReadDbContext readDbContext,
            AuthContext authContext,
            IMapper mapper,
            IQuestionRepository questionRepository,
            IExerciseQuestionRepository exerciseQuestionRepository,
            IExerciseRepository exerciseRepository,
            ITimeCodeExerciseRepository timeCodeExerciseRepository) : base(dbContext, readDbContext, authContext, mapper)
        {
            _questionRepository = questionRepository;
            _exerciseQuestionRepository = exerciseQuestionRepository;
            _exerciseRepository = exerciseRepository;
            _timeCodeExerciseRepository = timeCodeExerciseRepository;
        }

        public async Task<IList<SkillScores>> GetSkillScoresAsync(List<Guid> videoIds, EnumTimeCodeType timeCodeType)
        {
            if (videoIds == null || videoIds.Count == 0)
            {
                return new List<SkillScores>();
            }

            // 1) Multiplier theo số lần videoId xuất hiện
            var videoIdCounts = videoIds
                .GroupBy(x => x)
                .ToDictionary(g => g.Key, g => g.Count());

            var distinctVideoIds = videoIdCounts.Keys.ToList();

            // 2) Query base: total theo VideoId + Skill (mỗi exercise đóng góp question/count)
            //    NOTE: viết theo join để tránh subquery Count/Sum trong select (tối ưu SQL)
            var rows = await (from vtc in ReadQueryable.AsNoTracking()
                              join te in _timeCodeExerciseRepository.ReadQueryable.AsNoTracking()
                                  on vtc.Id equals te.VideoTimeCodeId
                              join ex in _exerciseRepository.ReadQueryable.AsNoTracking()
                                  on te.ExerciseId equals ex.Id
                              join eq in _exerciseQuestionRepository.ReadQueryable.AsNoTracking()
                                  on ex.Id equals eq.ExerciseId
                              join q in _questionRepository.ReadQueryable.AsNoTracking()
                                  on eq.QuestionId equals q.Id
                              where distinctVideoIds.Contains(vtc.VideoId)
                                    && vtc.TimeCodeType == timeCodeType
                                    && !q.Ungraded
                                    && q.QuestionType != EnumQuestionType.ExercisePreparation
                              select new
                              {
                                  vtc.VideoId,
                                  ex.SkillId,
                                  SkillFilePath = ex.Skill != null ? ex.Skill.FilePath : string.Empty,
                                  SkillName = ex.Skill != null ? ex.Skill.Name : string.Empty, // nếu Skill là navigation, nên Join skill table thay vì Include
                                  QuestionId = q.Id,
                                  q.CorrectTotal
                              }).ToListAsync();

            if (rows.Count == 0)
            {
                return new List<SkillScores>();
            }

            // 3) Gom theo (VideoId, Skill) => totals cho 1 lần xuất hiện của video đó
            var perVideoSkill = rows
                .GroupBy(x => new { x.VideoId, x.SkillId })
                .Select(g =>
                {
                    // TotalQuestion là số câu hỏi (distinct theo QuestionId)
                    var totalQuestion = g.Select(x => x.QuestionId).Distinct().Count();
                    var totalCount = g.Sum(x => x.CorrectTotal);

                    // nhân theo số lần videoId xuất hiện
                    var multiplier = videoIdCounts.TryGetValue(g.Key.VideoId, out var m) ? m : 1;

                    return new SkillScores
                    {
                        SkillFilePath = g.Where(x => x.SkillFilePath != null).FirstOrDefault()?.SkillFilePath,
                        SkillName = g.Where(x => x.SkillName != null).FirstOrDefault()?.SkillName,

                        SkillId = g.Key.SkillId,
                        TotalQuestion = totalQuestion * multiplier,
                        TotalCount = totalCount * multiplier
                    };
                })
                .ToList();

            // 4) Gom cuối theo Skill
            return perVideoSkill
                .GroupBy(x => new { x.SkillId })
                .Select(g => new SkillScores
                {
                    SkillFilePath = g.Where(x => x.SkillFilePath != null).FirstOrDefault()?.SkillFilePath,
                    SkillName = g.Where(x => x.SkillName != null).FirstOrDefault()?.SkillName,

                    SkillId = g.Key.SkillId,
                    TotalQuestion = g.Sum(x => x.TotalQuestion),
                    TotalCount = g.Sum(x => x.TotalCount)
                })
                .ToList();
        }
    }
}
