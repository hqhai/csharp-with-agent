// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.Reports
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.InteractionService;
    using Fsel.Course.Lms.Application.Services.InteractionService.Models;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetOverallReportByStudentQuery : IRequest<MethodResult<OverallReportModel>>
    {
        public Guid CourseId { get; set; }
        public Guid? StudentId { get; set; }
    }

    public class GetOverallReportByStudentQueryHandler : IRequestHandler<GetOverallReportByStudentQuery, MethodResult<OverallReportModel>>
    {
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly AuthContext _authContext;
        private readonly IInteractionService _interactionService;
        private readonly IUserService _userService;
        private readonly ILessonResultRepository _lessonResultRepository;

        public GetOverallReportByStudentQueryHandler(ICourseResultRepository courseResultRepository, IClassForumResultRepository classForumResultRepository, AuthContext authContext, IInteractionService interactionService, IUserService userService, ILessonResultRepository lessonResultRepository)
        {
            _courseResultRepository = courseResultRepository;
            _classForumResultRepository = classForumResultRepository;
            _authContext = authContext;
            _interactionService = interactionService;
            _userService = userService;
            _lessonResultRepository = lessonResultRepository;
        }

        public async Task<MethodResult<OverallReportModel>> Handle(GetOverallReportByStudentQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<OverallReportModel>();

            Guid? studentId;
            Guid? userId = null;
            if (_authContext.CurrentUserId != default)
            {
                var studentResult = await _userService.GetStudentByUserIdWithCacheAsync(_authContext.CurrentUserId);
                if (!studentResult.IsSuccessStatusCode)
                {
                    methodResult.AddError(studentResult.Error);
                    return methodResult;
                }
                studentId = studentResult.Content?.Result?.Id;
                userId = _authContext.CurrentUserId;
            }
            else
            {
                var studentResult = await _userService.GetStudentsByStudentIdsAsync(new List<Guid>() { request.StudentId ?? default });
                if (!studentResult.IsSuccessStatusCode)
                {
                    methodResult.AddError(studentResult.Error);
                    return methodResult;
                }
                var student = studentResult.Content?.Result;
                studentId = student?.FirstOrDefault()?.Id;
                userId = student?.FirstOrDefault()?.UserId;
            }

            var courseResult = await _courseResultRepository.Queryable.Include(p => p.Course).FirstOrDefaultAsync(p => p.CourseId == request.CourseId && p.StudentId == studentId, cancellationToken);
            if (courseResult == null || courseResult.Status != Domain.Enums.EnumResultStatus.Done)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var classForumResults = from cfr in _classForumResultRepository.Queryable
                                    join lr in _lessonResultRepository.Queryable on cfr.LessonResultId equals lr.Id
                                    where cfr.StudentId == studentId && lr.CourseResultId == courseResult.Id
                                    select cfr;

            var classForumResultIds = classForumResults.Select(p => p.Id).ToList();

            var aggregateNumberOfLikesAndCommentsResult = await _interactionService.AggregateNumberOfLikesAndComments(new AggregateNumberOfLikesAndCommentsQueryModel
            {
                UserId = userId ?? default,
                ClassForumResultIds = classForumResultIds,
                CourseId = request.CourseId
            });
            var aggregateNumberOfLikesAndComments = aggregateNumberOfLikesAndCommentsResult.Content?.Result;

            var overallReportModel = new OverallReportModel()
            {
                CourseLevel = courseResult.Course!.CourseLevel,
                OverallScore = (int)courseResult.Percent,
                OverallSkillReports = courseResult.SkillScores?.Select(s => new OverallSkillReportModel()
                {
                    Skill = s.Skill,
                    Value = (int)s.Percent
                }).ToList(),
                Give = new AggregateNumberOfLikesAndComments()
                {
                    LikeNumber = aggregateNumberOfLikesAndComments?.Give?.NumberLike ?? 0,
                    CommentNumber = aggregateNumberOfLikesAndComments?.Give?.NumberComment ?? 0
                },
                Receive = new AggregateNumberOfLikesAndComments()
                {
                    LikeNumber = aggregateNumberOfLikesAndComments?.Receive?.NumberLike ?? 0,
                    CommentNumber = aggregateNumberOfLikesAndComments?.Receive?.NumberComment ?? 0
                }
            };

            methodResult.Result = overallReportModel;
            return methodResult;
        }
    }
}
