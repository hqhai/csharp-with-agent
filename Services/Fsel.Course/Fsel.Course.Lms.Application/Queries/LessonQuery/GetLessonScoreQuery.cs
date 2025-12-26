// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.LessonQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetLessonScoreQuery : IRequest<MethodResult<LessonScoreModel>>
    {
        public Guid CourseId { get; set; }
        public Guid UnitId { get; set; }
        public Guid LessonId { get; set; }
    }

    public class GetLessonScoreQueryHandler : IRequestHandler<GetLessonScoreQuery, MethodResult<LessonScoreModel>>
    {
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly VideoConverter _videoConverter;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;

        public GetLessonScoreQueryHandler(
            AuthContext authContext,
            VideoConverter videoConverter,
            ILessonResultRepository lessonResultRepository,
            IUserService userService)
        {
            _authContext = authContext;
            _lessonResultRepository = lessonResultRepository;
            _videoConverter = videoConverter;
            _userService = userService;
        }

        public async Task<MethodResult<LessonScoreModel>> Handle(GetLessonScoreQuery request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<LessonScoreModel>();
            var lessonScore = new LessonScoreModel();

            var student = await _userService.GetStudentByUserIdWithCacheAsync(_authContext.CurrentUserId);
            if (!student.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }
            var studentId = student?.Content?.Result?.Id;
            var lessonResult = await _lessonResultRepository.Queryable.Include(x => x.VideoResults).Where(x => x.CourseId == request.CourseId && x.UnitId == request.UnitId && x.LessonId == request.LessonId && x.StudentId == studentId).FirstOrDefaultAsync(cancellationToken);
            if (lessonResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(lessonResult));
                return methodResult;
            }
            if (lessonResult.VideoResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(lessonResult.VideoResult));
                return methodResult;
            }
            var timeCodeScoreResult = await _videoConverter.GetVideoSkillScores(lessonResult.VideoResult, cancellationToken);

            var timeCodeScores = new List<TimeCodeScoreModel>();

            foreach (var item in timeCodeScoreResult.Item1)
            {
                if (item.SkillScores != null && item.SkillScores.Count > 0)
                {
                    var correctCountTimeCode = item.SkillScores.Sum(x => x.CorrectCount);
                    var totalCountTimeCode = item.SkillScores.Sum(x => x.TotalCount);

                    timeCodeScores.Add(new TimeCodeScoreModel
                    {
                        Type = item.Type,
                        SkillScores = item.SkillScores,
                        Percent = totalCountTimeCode > 0 ? NumberHelper.GetPercent(correctCountTimeCode, totalCountTimeCode) : default
                    });
                }
            }
            var timeCodeScoreStadalon = timeCodeScores.FirstOrDefault(x => x.Type == EnumTimeCodeType.Standalone);

            if (timeCodeScoreStadalon != null && timeCodeScoreStadalon.SkillScores != null)
            {
                var correctCount = timeCodeScoreStadalon.SkillScores.Sum(x => x.CorrectCount);
                var totalCount = timeCodeScoreStadalon.SkillScores.Sum(x => x.TotalCount);
                lessonScore.TotalCount = totalCount;
                lessonScore.CorrectCount = correctCount;
                lessonScore.Percent = totalCount > 0 ? NumberHelper.GetPercent(correctCount, totalCount) : default;
                lessonScore.SkillScores = timeCodeScoreStadalon.SkillScores;
            }

            lessonScore.TimeCodeScores = timeCodeScores.Where(x => x.Type != EnumTimeCodeType.Standalone).ToList();
            methodResult.Result = lessonScore;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
