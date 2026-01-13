namespace Fsel.Course.Lms.Application.Queries.ProgressQuery.V1i2
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public sealed class GetLearningPathwayQuery : IRequest<MethodResult<LearningPathwayResponse>>
    {
        public Guid CourseId { get; set; }
    }

    public sealed class GetLearningPathwayQueryHandler
        : IRequestHandler<GetLearningPathwayQuery, MethodResult<LearningPathwayResponse>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ICategoryRepository _programRepository;

        public GetLearningPathwayQueryHandler(
            ICourseRepository courseRepository,
            ICategoryRepository programRepository)
        {
            _courseRepository = courseRepository;
            _programRepository = programRepository;
        }

        public async Task<MethodResult<LearningPathwayResponse>> Handle(
            GetLearningPathwayQuery request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var methodResult = new MethodResult<LearningPathwayResponse>();

            // 1) Course
            var course = await _courseRepository.ReadQueryable
                .AsNoTracking()
                .Where(x => x.Id == request.CourseId)
                .Select(x => new
                {
                    x.Id,
                    x.ProgramId,
                    x.LevelId
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), "Course");
                return methodResult;
            }

            if (course.ProgramId == Guid.Empty)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), "ProgramId");
                return methodResult;
            }

            // 2) Program + Levels
            var program = await _programRepository.ReadQueryable
                .AsNoTracking()
                .Where(x => x.Id == course.ProgramId)
                .Select(x => new
                {
                    x.Id,
                    Levels = x.Levels.Select(l => new LevelLite
                    {
                        Id = l.Id,
                        Name = l.Name,
                        Code = l.Code,
                        LevelOrder = l.LevelOrder,
                        CreatedDate = l.CreatedDate
                    }).ToList()
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (program == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), "Program");
                return methodResult;
            }

            if (program.Levels == null || program.Levels.Count == 0)
            {
                methodResult.Result = new LearningPathwayResponse
                {
                    CourseId = course.Id,
                    ProgramId = program.Id,
                    CurrentRank = 0,
                    CurrentLevelId = course.LevelId,
                    Levels = new List<LearningPathwayLevelDto>()
                };
                return methodResult;
            }

            // 3) RULE: cùng LevelOrder -> lấy level có CreatedDate mới nhất
            var levelsByRank = program.Levels
                .Where(l => l.LevelOrder > 0)
                .GroupBy(l => l.LevelOrder)
                .Select(g => g
                    .OrderByDescending(x => x.CreatedDate)
                    .ThenByDescending(x => x.Id)
                    .First())
                .OrderBy(x => x.LevelOrder)
                .ToList();

            if (levelsByRank.Count == 0)
            {
                methodResult.Result = new LearningPathwayResponse
                {
                    CourseId = course.Id,
                    ProgramId = program.Id,
                    CurrentRank = 0,
                    CurrentLevelId = course.LevelId,
                    Levels = new List<LearningPathwayLevelDto>()
                };
                return methodResult;
            }

            // 4) Current rank: lấy từ Course.LevelId, không có thì min rank
            var currentRank = ResolveCurrentRank(course.LevelId, program.Levels, levelsByRank);

            // Nếu currentRank < min rank hoặc > max rank => normalize về range hợp lệ để Active đúng
            currentRank = NormalizeCurrentRank(levelsByRank, currentRank);

            var currentLevelPicked = PickCurrentLevelId(course.LevelId, levelsByRank, currentRank);

            methodResult.Result = new LearningPathwayResponse
            {
                CourseId = course.Id,
                ProgramId = program.Id,
                CurrentRank = currentRank,
                CurrentLevelId = currentLevelPicked,
                Levels = levelsByRank
                    .OrderBy(x => x.LevelOrder)
                    .Select(x => new LearningPathwayLevelDto
                    {
                        LevelId = x.Id,
                        Name = x.Name,
                        Code = x.Code,
                        Rank = x.LevelOrder,

                        // ✅ level nhỏ hơn hoặc bằng level hiện tại => active
                        IsActive = x.LevelOrder <= currentRank,

                        IsCurrent = currentLevelPicked.HasValue && x.Id == currentLevelPicked.Value
                    })
                    .ToList()
            };

            return methodResult;
        }

        private static int ResolveCurrentRank(
            Guid? courseLevelId,
            IReadOnlyList<LevelLite> allProgramLevels,
            IReadOnlyList<LevelLite> levelsByRank)
        {
            if (courseLevelId.HasValue)
            {
                var lvl = allProgramLevels.FirstOrDefault(l => l.Id == courseLevelId.Value);
                if (lvl != null && lvl.LevelOrder > 0)
                {
                    return lvl.LevelOrder;
                }
            }

            return levelsByRank.Min(x => x.LevelOrder);
        }

        private static Guid? PickCurrentLevelId(
            Guid? courseLevelId,
            IReadOnlyList<LevelLite> levelsByRank,
            int currentRank)
        {
            if (courseLevelId.HasValue)
                return courseLevelId;

            // fallback: level đại diện của rank (newest per rank)
            var lvl = levelsByRank.FirstOrDefault(x => x.LevelOrder == currentRank);
            return lvl?.Id;
        }

        private static int NormalizeCurrentRank(IReadOnlyList<LevelLite> levelsByRank, int currentRank)
        {
            var min = levelsByRank.Min(x => x.LevelOrder);
            var max = levelsByRank.Max(x => x.LevelOrder);

            if (currentRank < min)
                return min;
            if (currentRank > max)
                return max;
            return currentRank;
        }

        private sealed class LevelLite
        {
            public Guid Id { get; set; }
            public string? Name { get; set; }
            public string? Code { get; set; }
            public int LevelOrder { get; set; }
            public DateTime CreatedDate { get; set; }
        }
    }

    public sealed class LearningPathwayResponse
    {
        public Guid CourseId { get; set; }
        public Guid ProgramId { get; set; }

        public Guid? CurrentLevelId { get; set; }
        public int CurrentRank { get; set; }

        public IList<LearningPathwayLevelDto> Levels { get; set; } = new List<LearningPathwayLevelDto>();
    }

    public sealed class LearningPathwayLevelDto
    {
        public Guid LevelId { get; set; }
        public string? Name { get; set; }
        public string? Code { get; set; }

        /// <summary>Rank hiển thị (mapping từ LevelOrder)</summary>
        public int Rank { get; set; }

        /// <summary>✅ Level <= CurrentRank => Active</summary>
        public bool IsActive { get; set; }

        public bool IsCurrent { get; set; }
    }
}
