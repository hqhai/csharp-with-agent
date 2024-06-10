// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.Reports
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Models;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.InteractionService;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetOverallReportByStudentQuery : IRequest<MethodResult<OverallReportModel>>
    {
        public Guid CourseId { get; set; }
        public Guid? StudentId { get; set; }
    }

    public class GetOverallReportByStudentQueryHandler : IRequestHandler<GetOverallReportByStudentQuery, MethodResult<OverallReportModel>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly AuthContext _authContext;
        private readonly ISystemService _systemService;
        private readonly IInteractionService _interactionService;
        private readonly IUserService _userService;

        public GetOverallReportByStudentQueryHandler(ICourseRepository courseRepository, ICourseResultRepository courseResultRepository, AuthContext authContext, ISystemService systemService, IInteractionService interactionService, IUserService userService, IClassForumResultRepository classForumResultRepository)
        {
            _courseRepository = courseRepository;
            _courseResultRepository = courseResultRepository;
            _authContext = authContext;
            _systemService = systemService;
            _interactionService = interactionService;
            _userService = userService;
            _classForumResultRepository = classForumResultRepository;
        }

        public async Task<MethodResult<OverallReportModel>> Handle(GetOverallReportByStudentQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<OverallReportModel>();

            Guid? studentId;
            Guid? userId = null;
            if (_authContext.CurrentUserId != default)
            {
                var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
                if (!studentResult.IsSuccessStatusCode)
                {
                    methodResult.AddError(studentResult.Error);
                    return methodResult;
                }
                studentId = studentResult.Content?.Result?.Id;
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
                userId = student?.FirstOrDefault()?.Human?.UserId;
            }

            var courseResult = await _courseResultRepository.Queryable.Include(p => p.Course).FirstOrDefaultAsync(p => p.CourseId == request.CourseId && p.StudentId == studentId, cancellationToken);
            if (courseResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var classForumResultIds = await _classForumResultRepository.Queryable.Include(p => p.LessonResult).Where(p => p.LessonResult != null && p.LessonResult.CourseId == request.CourseId && p.StudentId == studentId).Select(p => p.Id).ToListAsync(cancellationToken);

            var give = await Give(userId ?? default, request.CourseId);

            var receive = await Receive(classForumResultIds);

            var overallReportModel = new OverallReportModel()
            {
                CourseLevel = courseResult.Course!.CourseLevel,
                OverallScore = (int)courseResult.Percent,
                OverallSkillReports = courseResult.SkillScores?.Select(s => new OverallSkillReportModel()
                {
                    Skill = s.Skill,
                    Value = (int)s.Percent
                }).ToList(),
                Give = new OverallClassForumReportModel()
                {
                    LikeNumber = give.Item2,
                    CommentNumber = give.Item1
                },
                Receive = new OverallClassForumReportModel()
                {
                    LikeNumber = receive.Item2,
                    CommentNumber = receive.Item1
                }
            };

            methodResult.Result = overallReportModel;
            return methodResult;
        }

        private async Task<(int, int)> Give(Guid userId, Guid courseId)
        {
            var classForumResultIds = new List<Guid>();

            var commentResults = await _interactionService.ExecuteListCommentQueryAsync(new BaseQueryModel()
            {
                Filters = new List<GenericFilterModel>() {
                    new GenericFilterModel()
                    {
                        Property = "UserId",
                        Operator = EnumFilterOperator.Equal,
                        Value = userId
                    },
                    new GenericFilterModel()
                    {
                        Property = "Type",
                        Operator = EnumFilterOperator.NotEqual,
                        Value = EnumInteractionType.DiscussionBoard
                    }
                }
            });

            var comments = commentResults.Content?.Result?.Select(p => p.ObjectId).ToList();
            if (comments != null && comments.Count > 0)
            {
                classForumResultIds.AddRange(comments);
            }

            var actionResults = await _interactionService.ExecuteListActionQueryAsync(new BaseQueryModel()
            {
                Filters = new List<GenericFilterModel>() {
                    new GenericFilterModel()
                    {
                        Property = "UserId",
                        Operator = EnumFilterOperator.Equal,
                        Value = userId
                    },
                    new GenericFilterModel()
                    {
                        Property = "Type",
                        Operator = EnumFilterOperator.Equal,
                        Value = EnumInteractionActionType.Like
                    },
                    new GenericFilterModel()
                    {
                        Property = "BusinessType",
                        Operator = EnumFilterOperator.Equal,
                        Value = EnumInteractionType.ClassForum
                    }
                }
            });
            var actions = actionResults.Content?.Result?.Select(p => p.ObjectId).ToList();
            if (actions != null && actions.Count > 0)
            {
                classForumResultIds.AddRange(actions);
            }
            classForumResultIds = classForumResultIds.Distinct().ToList();

            var classForumResult = await _classForumResultRepository.Queryable.Include(p => p.LessonResult).Where(p => p.LessonResult != null && p.LessonResult.CourseId == courseId && classForumResultIds.Contains(p.Id)).ToListAsync(CancellationToken.None);

            int commentNumber = classForumResult.Where(p => comments != null && comments.Count > 0 && comments.Contains(p.Id)).Count();
            int commentLike = classForumResult.Where(p => actions != null && actions.Count > 0 && actions.Contains(p.Id)).Count();

            return (commentNumber, commentLike);
        }

        private async Task<(int, int)> Receive(List<Guid>? classForumResultIds)
        {
            var commentResults = await _interactionService.ExecuteListCommentQueryAsync(new BaseQueryModel()
            {
                Filters = new List<GenericFilterModel>() {
                    new GenericFilterModel()
                    {
                        Property = "ObjectId",
                        Operator = EnumFilterOperator.In,
                        Value = classForumResultIds
                    }
                }
            });

            var actionResults = await _interactionService.ExecuteListActionQueryAsync(new BaseQueryModel()
            {
                Filters = new List<GenericFilterModel>() {
                    new GenericFilterModel()
                    {
                        Property = "ObjectId",
                        Operator = EnumFilterOperator.In,
                        Value = classForumResultIds
                    },
                    new GenericFilterModel()
                    {
                        Property = "Type",
                        Operator = EnumFilterOperator.Equal,
                        Value = EnumInteractionActionType.Like
                    }
                }
            });

            return (commentResults.Content?.Result?.Count ?? 0, actionResults.Content?.Result?.Count ?? 0);
        }
    }
}
