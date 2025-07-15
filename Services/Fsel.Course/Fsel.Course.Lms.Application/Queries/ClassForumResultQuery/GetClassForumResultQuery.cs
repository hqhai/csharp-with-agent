// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ClassForumResultQuery
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Repositories;
    using Fsel.Course.Lms.Application.Services.InteractionService;
    using Fsel.Course.Lms.Application.Services.InteractionService.Models;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetClassForumResultQuery : IRequest<MethodResult<ClassForumResultModel>>
    {
        public Guid ClassForumResultId { get; set; }
    }

    public class GetClassForumResultQueryHandler : IRequestHandler<GetClassForumResultQuery, MethodResult<ClassForumResultModel>>
    {
        private readonly AuthContext _authContext;
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IInteractionService _interactionService;

        public GetClassForumResultQueryHandler(AuthContext authContext, IClassForumResultRepository classForumResultRepository, IMapper mapper, IUserService userService, IInteractionService interactionService)
        {
            _authContext = authContext;
            _classForumResultRepository = classForumResultRepository;
            _mapper = mapper;
            _userService = userService;
            _interactionService = interactionService;
        }

        public async Task<MethodResult<ClassForumResultModel>> Handle(GetClassForumResultQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ClassForumResultModel>();
            var classForumResult = await _classForumResultRepository.GetByIdAsync(request.ClassForumResultId);

            if (classForumResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(classForumResult));
                return methodResult;
            }
            if (_authContext.Roles!.Contains(EnumRole.CSO.ToString()))
            {
                var csoResults = await _userService.GetCSOByUserId(_authContext.CurrentUserId);
                var csoId = csoResults.Content?.Result?.Id;
                if (classForumResult.CheckStartDate.HasValue && classForumResult.CheckStartDate.Value.AddMinutes(30) < DateTime.UtcNow)
                {
                    classForumResult.CheckCsoId = null;
                    classForumResult.CheckStartDate = null;
                }
                else
                {
                    if (classForumResult.CheckCsoId == null)
                    {
                        classForumResult.CheckCsoId = csoId;
                        classForumResult.CheckStartDate = DateTime.UtcNow;
                    }
                }
            }

            if (_authContext.Roles!.Contains(EnumRole.Teacher.ToString()))
            {
                var teacherResult = await _userService.GetTeacherByUserIdAsync(_authContext.CurrentUserId);
                var teacherId = teacherResult.Content?.Result?.Id;
                if (classForumResult.GradingStartDate.HasValue && classForumResult.GradingStartDate.Value.AddMinutes(30) < DateTime.UtcNow)
                {
                    classForumResult.GradingTeacherId = null;
                    classForumResult.GradingStartDate = null;
                }
                else
                {
                    if (classForumResult.GradingTeacherId == null)
                    {
                        classForumResult.GradingTeacherId = teacherId;
                        classForumResult.GradingStartDate = DateTime.UtcNow;
                    }
                }
            }

            await _classForumResultRepository.BulkUpdateList(new List<ClassForumResult> { classForumResult }, bulk =>
            {
                bulk.IgnoreOnUpdateExpression = c => new { c.ClassForumId, c.StudentId, c.LessonResultId };
            });

            classForumResult = await _classForumResultRepository.Queryable
                .Include(x => x.ClassForumDetailResults)
                .ThenInclude(x => x.ClassForumResultFiles)
                .Include(x => x.LessonResult)
                .ThenInclude(x => x!.Lesson)
                .ThenInclude(x => x!.UnitLessons)
                .Include(x => x.LessonResult)
                .ThenInclude(x => x!.Unit)
                .ThenInclude(x => x!.CourseUnitMockTests)
                .Include(x => x.LessonResult)
                .ThenInclude(x => x!.Course)
                .Include(x => x.ClassForum)
                .ThenInclude(x => x!.ClassForumFiles)
                .Include(x => x.ClassForumResultFiles)
                .Include(x => x.ClassForumScores)
                .Where(x => x.Id == request.ClassForumResultId)
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);
            if (classForumResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(classForumResult));
                return methodResult;
            }
            var lesson = classForumResult.LessonResult?.Lesson?.UnitLessons.FirstOrDefault(y => y.UnitId == classForumResult.LessonResult.UnitId);
            var unit = classForumResult.LessonResult?.Unit?.CourseUnitMockTests.FirstOrDefault(y => y.CourseId == classForumResult.LessonResult.CourseId);
            var course = classForumResult.LessonResult?.Course;
            var currentUnitId = classForumResult.LessonResult?.UnitId;
            var classForumResultModel = new ClassForumResultModel
            {
                Id = classForumResult.Id,
                Content = classForumResult.Content,
                LessonResultId = classForumResult.LessonResultId,
                CurrentUnitId = currentUnitId ?? default,
                WordContent = classForumResult.WordContent,
                WordCount = classForumResult.WordCount,
                TimeCount = classForumResult.TimeCount,
                CorrectCount = classForumResult.CorrectCount,
                CorrectTotal = classForumResult.CorrectTotal,
                SkillScores = classForumResult.SkillScores,
                Status = classForumResult.Status,
                ClassForumId = classForumResult.ClassForumId,
                ClassForum = _mapper.Map<ClassForumModel>(classForumResult.ClassForum),
                CreatedDate = classForumResult.CreatedDate,
                CheckStartDate = classForumResult.CheckStartDate,
                GradingStartDate = classForumResult.GradingStartDate,
                CheckCsoId = classForumResult.CheckCsoId,
                UnitId = unit?.Id ?? default,
                CreatedUserId = classForumResult.CreatedUserId,
                CourseId = course?.Id ?? default,
                GradingTeacherId = classForumResult.GradingTeacherId ?? default,
                ClassForumResultFiles = _mapper.Map<IList<ClassForumResultFileModel>>(classForumResult.ClassForumResultFiles),
                ClassForumScores = classForumResult.ClassForumScores == null ? null : classForumResult.ClassForumScores.Select(x => new ClassForumScoreModel
                {
                    Id = x.Id,
                    Feedback = x.Feedback,
                    Criteria = x.Criteria,
                    Score = x.Score
                }).ToList(),
                PostArea = "L" + lesson?.DisplayOrder + "_" + "U" + unit?.Number + "_" + course?.Code,
                ClassForumDetailResults = classForumResult.ClassForumDetailResults.Select(x =>
                {
                    x.Score = GetTargetCount(x, classForumResult);
                    return _mapper.Map<ClassForumDetailResultModel>(x);
                }).ToList(),
            };
            var studentResult = await _userService.GetStudentByUserIdAsync(classForumResultModel.CreatedUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }
            var student = studentResult?.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }
            if (!student.CourseLevel.HasValue)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student.CourseLevel));
                return methodResult;
            }
            classForumResultModel.CourseLevel = student.CourseLevel.Value;

            List<Guid> classForumResultIds = new List<Guid>() { classForumResultModel.Id };
            var actionsResult = await _interactionService.GetsActionAsync(new InteractionActionCommandModel { ObjectIds = classForumResultIds, UserId = _authContext.CurrentUserId });
            var actions = actionsResult.Content?.Result;
            var action = actions?.FirstOrDefault(x => x.ObjectId == classForumResultModel.Id);
            if (action != null)
            {
                classForumResultModel.CreatedFullName = student.Human?.FullName;
                classForumResultModel.LikeNumber = action.LikeNumber;
                classForumResultModel.CommentNumber = action.CommentNumber;
                classForumResultModel.IsLiked = action.IsLiked;
            }

            methodResult.Result = classForumResultModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private static int GetTargetCount(ClassForumDetailResult classForumDetailResult, ClassForumResult classForumResult)
        {
            int targetScore = default;
            var classForum = classForumResult.ClassForum;
            if (classForum?.CourseSkill == EnumCourseSkill.Writing && classForum?.TaggetWordLimit <= classForumDetailResult.WordCount)
            {
                ++targetScore;
            }
            if (classForum?.CourseSkill == EnumCourseSkill.Speaking && classForum?.TaggetTimeLimit <= classForumDetailResult.TimeCount)
            {
                ++targetScore;
            }
            return targetScore;
        }
    }
}
