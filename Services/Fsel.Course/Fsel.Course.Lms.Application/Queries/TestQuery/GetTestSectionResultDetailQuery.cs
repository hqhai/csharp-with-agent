// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.TestQuery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Common.ActionResults;
    using Domain.IRepositories;
    using Domain.Models.EntityModels;
    using Domain.Models.EntityModels.PlacementTestModels;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Models.EntityModels.V1i2;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.CacheServices;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetTestSectionResultDetailQuery : IRequest<MethodResult<SectionStateModel>>
    {
        public Guid TestSectionResultId { get; set; }
    }

    public class GetTestSectionResultDetailQueryHandler
        : IRequestHandler<GetTestSectionResultDetailQuery, MethodResult<SectionStateModel>>
    {
        private readonly ITestSectionResultRepository _testSectionResultRepository;
        private readonly ITestSectionCachingService _testSectionCachingService;
        private readonly ITestSectionRepository _testSectionRepository;

        public GetTestSectionResultDetailQueryHandler(
            ITestSectionResultRepository testSectionResultRepository,
            ITestSectionCachingService testSectionCachingService,
            ITestSectionRepository testSectionRepository)
        {
            _testSectionResultRepository = testSectionResultRepository;
            _testSectionCachingService = testSectionCachingService;
            _testSectionRepository = testSectionRepository;
        }

        public async Task<MethodResult<SectionStateModel>> Handle(
            GetTestSectionResultDetailQuery request,
            CancellationToken cancellationToken)
        {
            var testSectionResult = await _testSectionResultRepository.ReadQueryable
                .AsNoTracking()
                .Include(x => x.TestResult)
                .Include(x => x.TestSection)
                .Include(x => x.TestAnswers)
                .FirstOrDefaultAsync(x => x.Id == request.TestSectionResultId, cancellationToken);

            if (testSectionResult?.TestResult is null)
            {
                return new MethodResult<SectionStateModel>();
            }

            if (!testSectionResult.TestSectionId.HasValue || testSectionResult.TestSectionId.Value == Guid.Empty)
            {
                return new MethodResult<SectionStateModel>();
            }

            var rootSectionId = testSectionResult.TestSectionId.Value;

            // 1) cache subtree sections + questionIds
            var cached = await GetTestSectionTreeCachedAsync(rootSectionId);
            if (cached.Sections.Count == 0)
            {
                return new MethodResult<SectionStateModel>();
            }

            var rootSection = cached.Sections.FirstOrDefault(x => x.Id == rootSectionId);
            if (rootSection is null)
            {
                return new MethodResult<SectionStateModel>();
            }

            // Nếu không có question thì map bình thường (không cần query)
            var questionById = new Dictionary<Guid, Question>();

            var childrenMap = cached.Sections
                .Where(s => s.ParentId.HasValue)
                .GroupBy(s => s.ParentId!.Value)
                .ToDictionary(g => g.Key, g => g.OrderBy(x => x.DisplayOrder).ToList());

            var answerByQuestionId = (testSectionResult.TestAnswers ?? new List<TestAnswer>())
                .Where(x => x.QuestionId.HasValue && x.QuestionId.Value != Guid.Empty)
                .GroupBy(a => a.QuestionId!.Value)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(x => x.UpdatedDate ?? x.CreatedDate).FirstOrDefault());

            // ✅ NEW: get TestSectionResults nhỏ (con) để map result cho section con nếu có
            var sectionIds = cached.Sections.Select(s => s.Id).ToList();

            var sectionResultBySectionId = await _testSectionResultRepository.ReadQueryable
                .AsNoTracking()
                .Where(r =>
                    r.TestResultId == testSectionResult.TestResultId &&
                    r.TestSectionId.HasValue &&
                    sectionIds.Contains(r.TestSectionId.Value))
                .ToDictionaryAsync(r => r.TestSectionId!.Value, r => r, cancellationToken);

            var rootModel = MapSectionNode(
                rootSection,
                childrenMap,
                cached.QuestionIdsBySectionId,
                answerByQuestionId,
                questionById,
                sectionResultBySectionId);

            return new MethodResult<SectionStateModel>(rootModel);
        }

        private async Task<CachedSectionTreeModel> GetTestSectionTreeCachedAsync(Guid rootSectionId)
        {
            return await _testSectionCachingService.GetOrSetAsync(rootSectionId.ToString(), async (_, token) =>
            {
                // Lấy subtree sections (flat)
                var sections = await GetTestSectionTreeAsync(rootSectionId);
                if (sections.Count == 0)
                {
                    return new CachedSectionTreeModel();
                }

                var query = sections
                    .Where(x => x.TestSectionQuestions != null && x.TestSectionQuestions.Any())
                    .SelectMany(x => x.TestSectionQuestions);

                var questionPairs = query
                    .Select(q => new { SectionId = q.TestSectionId, q.QuestionId })
                    .ToList();

                var questionIdsBySectionId = questionPairs
                    .Where(x => x.QuestionId != Guid.Empty)
                    .GroupBy(x => x.SectionId)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(x => x.QuestionId).Distinct().ToList());

                return new CachedSectionTreeModel
                {
                    Sections = sections.ToList(),
                    QuestionIdsBySectionId = questionIdsBySectionId
                };
            });
        }

        // =========================
        // Tree loading (flat subtree)
        // =========================
        private async Task<IList<TestSection>> GetTestSectionTreeAsync(Guid rootSectionId)
        {
            var result = new List<TestSection>();

            var root = await _testSectionRepository.ReadQueryable
                .Include(x => x.TestSectionQuestions)
                .ThenInclude(x => x.Question)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == rootSectionId);

            if (root is null)
            {
                return result;
            }

            result.Add(root);

            var frontier = new List<Guid> { rootSectionId };

            while (frontier.Count > 0)
            {
                var parents = frontier;
                frontier = new List<Guid>();

                var children = await _testSectionRepository.ReadQueryable
                    .Include(x => x.TestSectionQuestions)
                    .ThenInclude(x => x.Question)
                    .AsNoTracking()
                    .Where(x => x.ParentId.HasValue && parents.Contains(x.ParentId.Value))
                    .OrderBy(x => x.DisplayOrder)
                    .ToListAsync();

                if (children.Count == 0)
                {
                    break;
                }

                result.AddRange(children);
                frontier.AddRange(children.Select(x => x.Id));
            }

            return result;
        }

        // =========================
        // Mapping: Section -> (Section children...) + (Question leaf...)
        // =========================
        private SectionStateModel MapSectionNode(
            TestSection node,
            Dictionary<Guid, List<TestSection>> childrenMap,
            Dictionary<Guid, List<Guid>> questionsBySectionId,
            Dictionary<Guid, TestAnswer?> answerByQuestionId,
            Dictionary<Guid, Question> questionById,
            Dictionary<Guid, TestSectionResult> sectionResultBySectionId)
        {
            // ✅ NEW: ưu tiên result của node nếu có, không có thì fallback root (hoặc null như luồng cũ)
            sectionResultBySectionId.TryGetValue(node.Id, out var nodeResult);

            var model = new SectionStateModel
            {
                SkillScores = nodeResult?.SkillScores,
                CorrectTotal = nodeResult?.CorrectTotal ?? 0,
                HighestStreak = nodeResult?.HighestStreak,

                SectionResultId = nodeResult?.Id,
                Status = nodeResult?.Status ?? EnumResultStatus.New,
                UpdatedDate = nodeResult?.UpdatedDate,
                Name = node.Name,
                CorrectCount = nodeResult?.CorrectCount ?? 0,
                PercentResult = nodeResult?.Percent ?? 0,
                Config = node.Config,
                TestLayoutType = node.LayoutType,
                TotalCount = nodeResult?.CorrectTotal ?? 0,
                WorkingTime = nodeResult?.WorkingTime ?? 0,
                SectionId = node.Id,
            };

            var children = new List<BaseTestStateModel>();

            // Section con
            if (childrenMap.TryGetValue(node.Id, out var sectionChildren) && sectionChildren.Count > 0)
            {
                foreach (var childSection in sectionChildren)
                {
                    children.Add(MapSectionNode(
                        childSection,
                        childrenMap,
                        questionsBySectionId,
                        answerByQuestionId,
                        questionById,
                        sectionResultBySectionId));
                }
            }

            // Question leaf + gắn Question object
            if (questionsBySectionId.TryGetValue(node.Id, out var questionIds) && questionIds.Count > 0)
            {
                foreach (var questionId in questionIds)
                {
                    children.Add(MapQuestionLeaf(questionId, answerByQuestionId, questionById));
                }
            }

            model.Children = children;
            return model;
        }

        private QuestionStateModel MapQuestionLeaf(
            Guid questionId,
            Dictionary<Guid, TestAnswer?> answerByQuestionId,
            Dictionary<Guid, Question> questionById)
        {
            var q = new QuestionStateModel
            {
                QuestionId = questionId,
                Status = EnumResultStatus.New,
                UpdatedDate = null,
            };

            if (answerByQuestionId.TryGetValue(questionId, out var ans) && ans is not null)
            {
                q.TestAnswerId = ans.Id;
                q.Answer = new AnswerModel
                {
                    Answer = ans.Answer,
                    CorrectCount = ans.CorrectCount,
                    IsCorrect = ans.IsCorrect,
                    Status = ans.Status
                };

                q.Status = ans.Status == EnumAnswerStatus.Done ? EnumResultStatus.Done : EnumResultStatus.New;
                q.UpdatedDate = ans.UpdatedDate ?? ans.CreatedDate;
            }

            return q;
        }
    }
}
