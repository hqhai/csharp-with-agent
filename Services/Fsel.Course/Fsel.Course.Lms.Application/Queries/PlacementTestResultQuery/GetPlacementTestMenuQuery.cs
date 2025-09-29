// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.PlacementTestResultQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using System.Text.Json;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Entities;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Helpers;
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;

    public class GetPlacementTestMenuQuery : IRequest<MethodResult<PlacementTestTreeModel>>
    {
        public Guid? StudentId { get; set; }

        public int DefaultMaxScore { get; set; }
    }

    public class GetPlacementTestMenuQueryHandler : IRequestHandler<GetPlacementTestMenuQuery, MethodResult<PlacementTestTreeModel>>
    {
        private readonly IPlacementTestResultRepository _placementTestResultRepository;
        private readonly ISectionGroupResultRepository _sectionGroupResultRepository;
        private readonly IPlacementTestRepository _placementTestRepository;
        private readonly IUserService _userService;

        public GetPlacementTestMenuQueryHandler(
            IPlacementTestResultRepository placementTestResultRepository,
            ISectionGroupResultRepository sectionGroupResultRepository,
            IPlacementTestRepository placementTestRepository,
            IUserService userService)
        {
            _placementTestResultRepository = placementTestResultRepository;
            _sectionGroupResultRepository = sectionGroupResultRepository;
            _placementTestRepository = placementTestRepository;
            _userService = userService;
        }

        public async Task<MethodResult<PlacementTestTreeModel>> Handle(GetPlacementTestMenuQuery request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<PlacementTestTreeModel>();

            try
            {
                // Query actual PlacementTestResults from database
                var placementTestResults = await _placementTestResultRepository.Queryable
                    .Include(x => x.SectionGroupResults)
                        .ThenInclude(sgr => sgr.SectionGroup)
                    .Where(x => x.StudentId == request.StudentId)
                    .OrderBy(x => x.CreatedDate)
                    .ToListAsync(cancellationToken);

                var placementTestTree = await BuildPlacementTestTree(request.StudentId, placementTestResults);

                methodResult.Result = placementTestTree;
                return methodResult;
            }
            catch (Exception ex)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.ServerError));
                return methodResult;
            }
        }

        private async Task<PlacementTestTreeModel> BuildPlacementTestTree(Guid? studentId, List<PlacementTestResult> placementTestResults)
        {
            // Tạo Tree structure cố định với 3 Parts
            var tree = new PlacementTestTreeModel
            {
                StudentId = studentId,
                Parts = CreateInitialTreeStructure()
            };

            // Nếu có PlacementTestResults, gán dữ liệu thực vào các Parts
            if (placementTestResults != null && placementTestResults.Any())
            {
                await LoadActualPlacementTestResultsIntoTree(tree, placementTestResults, studentId);
            }
            else
            {
                // Nếu không có PlacementTestResult
                tree.Parts = tree.Parts.Take(3).ToList();

                // Gán dữ liệu SkillScores mặc định cho mỗi part
                foreach (var part in tree.Parts)
                {
                    part.SkillScores = CreateDefaultSkillScores();
                }
            }

            return tree;
        }

        private static List<PlacementPartNode> CreateInitialTreeStructure()
        {
            var parts = new List<PlacementPartNode>();

            // Tạo 3 Parts cố định - chỉ có Part và Title, không có điểm số
            foreach (var partType in new[] { 1, 2, 3 })
            {
                parts.Add(new PlacementPartNode
                {
                    Part = partType,
                    Title = $"Phần {partType}",
                    IsExpanded = partType == 1, // Part 1 mở rộng mặc định
                    IsCompleted = false,
                    IsLock = false // Mặc định không khóa khi chưa có dữ liệu
                    // Không khởi tạo các thuộc tính điểm số khác để giữ giá trị mặc định
                });
            }

            return parts;
        }

        private static List<PlacementTestSkillScores> CreateDefaultSkillScores()
        {
            var defaultSkillScores = new List<PlacementTestSkillScores>();

            // Tạo SkillScores mặc định chỉ cho Reading, Listening, Vocabulary, Grammar, PT hiện tại chỉ có những kỹ năng này
            var defaultSkills = new[]
            {
                EnumCourseSkill.Reading,
                EnumCourseSkill.Listening,
                EnumCourseSkill.Vocabulary,
                EnumCourseSkill.Grammar
            };

            foreach (var skill in defaultSkills)
            {
                defaultSkillScores.Add(new PlacementTestSkillScores
                {
                    Skill = skill,
                    Scores = 0.0,
                    TotalCount = 0,
                    CorrectCount = 0,
                    TotalQuestion = 0,
                    CountQuestion = 0,
                    Percent = 0.0,
                    SectionGroupId = null
                });
            }

            return defaultSkillScores;
        }

        private async Task LoadActualPlacementTestResultsIntoTree(PlacementTestTreeModel tree, List<PlacementTestResult> placementTestResults, Guid? studentId)
        {
            // Gán từng PlacementTestResult vào từng Part trong Tree và kiểm tra lock status
            var partsToRemove = new List<PlacementPartNode>();

            for (int i = 0; i < placementTestResults.Count && i < tree.Parts.Count; i++)
            {
                var placementTestResult = placementTestResults[i];
                var partNode = tree.Parts[i];

                await MapActualPlacementTestResultToPart(placementTestResult, partNode, studentId);

                // Nếu part này bị lock, xóa tất cả part sau đó
                if (partNode.IsLock)
                {
                    // Thêm các part sau part hiện tại vào danh sách xóa
                    for (int j = i + 1; j < tree.Parts.Count; j++)
                    {
                        partsToRemove.Add(tree.Parts[j]);
                    }
                    break; // Dừng việc xử lý các part tiếp theo
                }
            }

            // Xóa các part không cần thiết
            foreach (var partToRemove in partsToRemove)
            {
                tree.Parts.Remove(partToRemove);
            }
        }

        private async Task MapActualPlacementTestResultToPart(PlacementTestResult placementTestResult, PlacementPartNode partNode, Guid? studentId)
        {
            // Map PlacementTestResult specific properties
            partNode.TotalQuestion = placementTestResult.TotalQuestion;
            partNode.CountQuestion = placementTestResult.CountQuestion;
            partNode.Level = placementTestResult.Level;
            partNode.PlacementTestId = placementTestResult.PlacementTestId;
            partNode.PlacementTestGroupResultId = placementTestResult.PlacementTestGroupResultId;

            // Map BaseResult properties
            partNode.CorrectCount = placementTestResult.CorrectCount;
            partNode.CorrectTotal = placementTestResult.CorrectTotal;
            partNode.Percent = placementTestResult.Percent;
            partNode.StudentId = placementTestResult.StudentId;

            // Use the entity's SkillScores property (already parsed by BaseScoreResult)
            if (placementTestResult.SkillScores != null && placementTestResult.SkillScores.Any())
            {
                // Convert entity SkillScores to PlacementTestSkillScores for consistency
                partNode.SkillScores = placementTestResult.SkillScores.Select(skillScore =>
                {
                    var placementTestSkillScore = new PlacementTestSkillScores
                    {
                        Skill = skillScore.Skill,
                        Scores = skillScore.Scores,
                        TotalCount = (int)skillScore.TotalCount,
                        CorrectCount = (int)skillScore.CorrectCount,
                        TotalQuestion = (int)skillScore.TotalQuestion,
                        CountQuestion = (int)skillScore.CountQuestion,
                        Percent = skillScore.Percent
                    };

                    // Map SectionGroupId theo skill từ SectionGroupResults
                    var sectionGroupResult = placementTestResult.SectionGroupResults?
                        .FirstOrDefault(sgr => sgr.SectionGroup?.CourseSkill == skillScore.Skill);

                    if (sectionGroupResult != null)
                    {
                        placementTestSkillScore.SectionGroupId = sectionGroupResult.SectionGroupId;
                    }

                    return placementTestSkillScore;
                }).ToList();
            }
            else if (!string.IsNullOrEmpty(placementTestResult.SkillScoresStr))
            {
                // Fallback to JSON parsing if entity SkillScores is null but string exists
                try
                {
                    var skillScoresData = JsonSerializer.Deserialize<List<PlacementTestSkillScores>>(placementTestResult.SkillScoresStr);

                    // Map SectionGroupId cho từng skill từ JSON data
                    if (skillScoresData != null)
                    {
                        foreach (var skillScore in skillScoresData)
                        {
                            var sectionGroupResult = placementTestResult.SectionGroupResults?
                                .FirstOrDefault(sgr => sgr.SectionGroup?.CourseSkill == skillScore.Skill);

                            if (sectionGroupResult != null)
                            {
                                skillScore.SectionGroupId = sectionGroupResult.SectionGroupId;
                            }
                        }
                    }

                    partNode.SkillScores = skillScoresData;
                }
                catch (JsonException)
                {

                }
            }

            // Set completion status based on actual data
            partNode.IsCompleted = placementTestResult.CorrectCount > 0;
            partNode.CompletedDate = placementTestResult.CreatedDate;

            // Calculate IsLock based on logic from CreatePlacementTestAnswerBySectionGroupCommand
            partNode.IsLock = await CalculateIsLockStatus(placementTestResult, studentId);
        }

        private async Task<bool> CalculateIsLockStatus(PlacementTestResult placementTestResult, Guid? studentId)
        {
            try
            {
                if (!studentId.HasValue || placementTestResult.PlacementTestId == null)
                {
                    return false;
                }

                // Get student information to calculate age
                var studentResult = await _userService.GetUserByStudentId(studentId.Value);
                if (!studentResult.IsSuccessStatusCode || studentResult.Content?.Result == null)
                {
                    return false;
                }

                var student = studentResult.Content.Result;
                if (student.Human?.Birthday == null)
                {
                    return false;
                }

                // Get placement test
                var placementTest = await _placementTestRepository.GetByIdAsync(placementTestResult.PlacementTestId.Value);
                if (placementTest == null)
                {
                    return false;
                }

                // Check if all 4 SectionGroupResults are done and have SkillScores
                const int numberOfDone = 4;
                var sectionGroupResults = await _sectionGroupResultRepository.Queryable
                    .Where(s => s.PlacementTestResultId == placementTestResult.Id && s.CreatedDate >= placementTestResult.CreatedDate)
                    .OrderBy(x => x.CreatedDate)
                    .ToListAsync();

                if (sectionGroupResults == null ||
                    sectionGroupResults.Count != numberOfDone ||
                    !sectionGroupResults.All(x => x.Status == EnumResultStatus.Done) ||
                    !sectionGroupResults.Where(x => x.SkillScores != null && x.SkillScores.Any()).Any())
                {
                    return false;
                }

                // Calculate age
                int age = DateTimeHelper.GetYearOld(student.Human.Birthday);

                // Get initial placement test result for level calculation
                var placementTestResultInitial = await _placementTestResultRepository.Queryable
                    .Where(x => x.StudentId == placementTestResult.StudentId)
                    .OrderBy(x => x.CreatedDate)
                    .FirstOrDefaultAsync();

                // Calculate isLockPT using the same logic as in Command
                var (currentLevel, isLockPT) = IeltsScoreHelper.GetLevelInScore(
                    placementTest.PlacementTestLevel,
                    placementTestResult.Percent,
                    IeltsScoreHelper.GetInitialAge(placementTestResultInitial?.Level, age));

                return isLockPT;
            }
            catch (Exception)
            {
                // If any error occurs, default to false (not locked)
                return false;
            }
        }
    }
}

