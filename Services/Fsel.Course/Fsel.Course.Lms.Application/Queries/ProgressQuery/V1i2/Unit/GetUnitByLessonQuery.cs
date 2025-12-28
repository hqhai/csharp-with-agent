// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ProgressQuery.V1i2.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetUnitByLessonQuery : IRequest<MethodResult<OverallScoreReportModel>>
    {
        public Guid LessonResultId { get; set; }
    }

    public class GetUnitByLessonQueryHandler : IRequestHandler<GetUnitByLessonQuery, MethodResult<OverallScoreReportModel>>
    {
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly IVideoTimeCodeResultRepository _videoTimeCodeResultRepository;
        private readonly IVideoTimeCodeRepository _videoTimeCodeRepository;

        public GetUnitByLessonQueryHandler(
            IVideoResultRepository videoResultRepository,
            IVideoTimeCodeResultRepository videoTimeCodeResultRepository,
            IVideoTimeCodeRepository videoTimeCodeRepository)
        {
            _videoResultRepository = videoResultRepository;
            _videoTimeCodeResultRepository = videoTimeCodeResultRepository;
            _videoTimeCodeRepository = videoTimeCodeRepository;
        }

        public async Task<MethodResult<OverallScoreReportModel>> Handle(GetUnitByLessonQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var methodResult = new MethodResult<OverallScoreReportModel>();
            var overallScoreReport = new OverallScoreReportModel();

            // ✅ 1 query: lấy videoIds, nếu rỗng => DataNotExist
            var videoIds = await _videoResultRepository.ReadQueryable
                .AsNoTracking()
                .Where(x => x.LessonResultId == request.LessonResultId)
                .Select(x => x.VideoId)
                .Distinct()
                .ToListAsync(cancellationToken);

            if (videoIds.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), "VideoResult");
                return methodResult;
            }

            var (scores, _) = await GetVideoSkillScores(request.LessonResultId, videoIds, cancellationToken);

            var skillScores = scores.FirstOrDefault(x => x.Type == EnumTimeCodeType.Standalone)?.SkillScores;
            if (skillScores is { Count: > 0 })
            {
                overallScoreReport.SkillScores = skillScores;
                overallScoreReport.CorrectCount = skillScores.Sum(x => x.CorrectCount);
                overallScoreReport.CorrectTotal = skillScores.Sum(x => x.TotalCount);
                overallScoreReport.CountQuestion = skillScores.Sum(x => x.CountQuestion);
                overallScoreReport.TotalQuestion = skillScores.Sum(x => x.TotalQuestion);
                overallScoreReport.CourseSkills = skillScores.Select(x => x.Skill).Distinct().ToList();
                overallScoreReport.Skills = skillScores
                    .Where(x => !string.IsNullOrEmpty(x.SkillName))
                    .Select(x => x.SkillName!)
                    .ToList();
            }

            methodResult.Result = overallScoreReport;
            return methodResult;
        }

        // overload giữ signature tiện dùng nội bộ
        private async Task<(IList<VideoSkillScores> Scores, bool IsMismatch)> GetVideoSkillScores(
            Guid lessonResultId,
            IList<Guid> videoIds,
            CancellationToken cancellationToken)
        {
            // =========================
            // A) Answers (VideoTimeCodeResult) - in-memory merge vì SkillScores thường là JSON/complex
            // =========================
            var vtrRows = await (
                from vr in _videoResultRepository.ReadQueryable.AsNoTracking()
                join vtr in _videoTimeCodeResultRepository.ReadQueryable.AsNoTracking()
                    on vr.Id equals vtr.VideoResultId
                join tc in _videoTimeCodeRepository.ReadQueryable.AsNoTracking()
                    on vtr.VideoTimeCodeId equals tc.Id
                where vr.LessonResultId == lessonResultId
                      && vtr.CreatedDate >= vr.CreatedDate
                      && (vr.Status != EnumResultStatus.Done || vtr.UpdatedDate <= vr.UpdatedDate)
                select new
                {
                    vr.VideoId,
                    TimeCodeType = tc.TimeCodeType,
                    vtr.CorrectTotal,
                    vtr.SkillScores
                }
            ).ToListAsync(cancellationToken);

            if (vtrRows.Count == 0)
            {
                return (new List<VideoSkillScores>(), false);
            }

            // Merge answers by (Type, SkillId)
            var answersMerged = vtrRows
                .Where(x => x.CorrectTotal > 0 && x.SkillScores != null && x.SkillScores.Any())
                .SelectMany(r => r.SkillScores!.Select(s => new
                {
                    Type = r.TimeCodeType,
                    s.Skill,
                    s.SkillId,
                    s.SkillName,
                    s.CorrectCount,
                    TotalAnswer = s.CountQuestion,
                    s.TokenReceived,
                    s.CorrectQuestion
                }))
                .Where(x => x.SkillId != null)
                .GroupBy(x => new { x.Type, x.SkillId })
                .Select(g => new
                {
                    g.Key.Type,
                    SkillId = g.Key.SkillId!,
                    Skill = g.Select(x => x.Skill).FirstOrDefault(),
                    SkillName = g.Select(x => x.SkillName).FirstOrDefault(),
                    CorrectCount = g.Sum(z => z.CorrectCount),
                    TotalAnswer = g.Sum(z => z.TotalAnswer),
                    TokenReceived = g.Sum(z => z.TokenReceived),
                    CorrectQuestion = g.Sum(z => z.CorrectQuestion),
                })
                .ToList();

            // =========================
            // B) Questions (source of truth) - ✅ aggregate SQL (không Include cây sâu)
            // =========================
            // Nếu videoIds truyền vào bị lệch so với vtrRows, vẫn ưu tiên theo lessonResultId
            var videoIdSet = videoIds is HashSet<Guid> hs ? hs : new HashSet<Guid>(videoIds);

            var questionsAgg = await (
                from tc in _videoTimeCodeRepository.ReadQueryable.AsNoTracking()
                where videoIdSet.Contains(tc.VideoId)
                from tce in tc.TimeCodeExercises
                let ex = tce.Exercise
                where ex != null && ex.SkillId != null
                from eq in ex.ExerciseQuestions
                let q = eq.Question
                where q != null
                      && !q.Ungraded
                      && q.QuestionType != EnumQuestionType.ExercisePreparation
                group new { tc, ex, q } by new
                {
                    tc.TimeCodeType,
                    ex!.CourseSkill,
                    SkillId = ex.SkillId!,
                    SkillName = ex.Skill != null ? ex.Skill.Name : null
                }
                into g
                select new
                {
                    Type = g.Key.TimeCodeType,
                    Skill = g.Key.CourseSkill,
                    g.Key.SkillId,
                    g.Key.SkillName,
                    TotalCount = g.Sum(x => x.q!.CorrectTotal),
                    TotalQuestion = g.Count()
                }
            ).ToListAsync(cancellationToken);

            // Filter có câu hỏi
            questionsAgg = questionsAgg
                .Where(x => x.TotalQuestion > 0)
                .ToList();

            // =========================
            // C) Compose result - O(n)
            // =========================
            var qDict = questionsAgg.ToDictionary(x => (x.Type, x.SkillId));
            var aDict = answersMerged.ToDictionary(x => (x.Type, x.SkillId));

            // types theo questionsAgg là đủ “chuẩn đề”; nếu muốn union với answers để debug vẫn giữ
            var types = questionsAgg.Select(x => x.Type)
                .Union(answersMerged.Select(x => x.Type))
                .Distinct()
                .ToList();

            var scoreResult = new List<VideoSkillScores>(types.Count);

            foreach (var type in types)
            {
                // lấy all skills thuộc type (theo questionsAgg)
                var keysOfType = questionsAgg
                    .Where(x => x.Type == type)
                    .Select(x => (x.Type, x.SkillId))
                    .ToList();

                if (keysOfType.Count == 0)
                {
                    // type chỉ có answers mà không có questions (data lệch) -> vẫn trả rỗng
                    scoreResult.Add(new VideoSkillScores { Type = type, SkillScores = new List<SkillScores>() });
                    continue;
                }

                var skillScoreList = new List<SkillScores>(keysOfType.Count);

                foreach (var key in keysOfType)
                {
                    if (!qDict.TryGetValue(key, out var q))
                    {
                        continue;
                    }
                    aDict.TryGetValue(key, out var a);

                    skillScoreList.Add(new SkillScores
                    {
                        Skill = q.Skill,
                        SkillId = q.SkillId,
                        SkillName = q.SkillName,
                        TotalCount = q.TotalCount,
                        TotalQuestion = q.TotalQuestion,
                        CorrectCount = a?.CorrectCount ?? 0,
                        CountQuestion = a?.TotalAnswer ?? 0,
                        TokenReceived = a?.TokenReceived ?? 0,
                    });
                }

                scoreResult.Add(new VideoSkillScores
                {
                    Type = type,
                    SkillScores = skillScoreList
                });
            }

            var totalQuestions = questionsAgg.Sum(x => x.TotalQuestion);
            var totalAnswers = answersMerged.Sum(x => x.TotalAnswer);
            var isMismatch = totalQuestions != totalAnswers;

            return (scoreResult, isMismatch);
        }

        // Giữ public method nếu nơi khác gọi
        public Task<(IList<VideoSkillScores> Scores, bool IsMismatch)> GetVideoSkillScores(
            Guid lessonResultId,
            CancellationToken cancellationToken)
            => GetVideoSkillScores(lessonResultId, new List<Guid>(), cancellationToken);
    }
}
