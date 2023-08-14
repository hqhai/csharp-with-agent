// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ClassForumQuery
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.InteractionService;
    using Fsel.Course.Lms.Application.Services.InteractionService.Models;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetClassForumQuery : IRequest<MethodResult<ClassForumByStudentModel>>
    {
        public Guid LessonId { get; set; }
        public Guid? LessonResultId { get; set; }
    }

    public class GetClassForumQueryHandler : IRequestHandler<GetClassForumQuery, MethodResult<ClassForumByStudentModel>>
    {
        private readonly IClassForumRepository _classForumRepository;
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;
        private readonly IMapper _mapper;
        private readonly ILessonRepository _lessonRepository;
        private readonly IInteractionService _interactionService;

        public GetClassForumQueryHandler(IClassForumRepository classForumRepository
            , IClassForumResultRepository classForumResultRepository
            , IUserService userService
            , AuthContext authContext
            , IMapper mapper
            , ILessonRepository lessonRepository
            , IInteractionService interactionService)
        {
            _classForumRepository = classForumRepository;
            _classForumResultRepository = classForumResultRepository;
            _userService = userService;
            _authContext = authContext;
            _mapper = mapper;
            _lessonRepository = lessonRepository;
            _interactionService = interactionService;
        }

        public async Task<MethodResult<ClassForumByStudentModel>> Handle(GetClassForumQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ClassForumByStudentModel> methodResult = new MethodResult<ClassForumByStudentModel>();

            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            var student = studentResult?.Content?.Result;
            if (studentResult == null || student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentResult));
                return methodResult;
            }
            var isLesson = await _lessonRepository.AnyAsync(request.LessonId);
            if (!isLesson)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(isLesson));
                return methodResult;
            }

            var classForum = await _classForumRepository.Queryable
                .Include(x => x.ClassForumFiles)
                .FirstOrDefaultAsync(x => x.LessonId == request.LessonId, cancellationToken);

            if (classForum == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(classForum));
                return methodResult;
            }
            var classForumByStudentModel = _mapper.Map<ClassForumByStudentModel>(classForum);

            var query = await _classForumResultRepository.Queryable
                .Include(x => x.ClassForumResultFiles)
                .Include(x => x.ClassForumScores)
                .Where(x => x.ClassForumId == classForum.Id)
                .ToListAsync(cancellationToken);

            var classForumResults = _mapper.Map<IList<ClassForumResultModel>>(query);
            if (classForumResults != null)
            {
                var actionsResult = await _interactionService.GetsActionAsync(new InteractionActionCommandModel { ObjectIds = classForumResults.Select(x => x.Id).ToList(), UserId = _authContext.CurrentUserId });
                var actions = actionsResult.Content?.Result;

                if (actions != null)
                {
                    classForumResults = classForumResults.Where(x => !actions.Any(n => n.IsDisable && n.ObjectId == x.Id)).ToList();
                    foreach (var item in classForumResults)
                    {
                        var action = actions.FirstOrDefault(x => x.ObjectId == item.Id);
                        item.CommentNumber = action?.CommentNumber;
                        item.LikeNumber = action?.LikeNumber;
                        item.IsLiked = action?.IsLiked;
                    }
                }

                var classForumResultCurrentStudent = classForumResults.FirstOrDefault(x => x.ClassForumId == classForum.Id && x.LessonResultId == request.LessonResultId);
                classForumByStudentModel.ClassForumResultCurrentStudent = classForumResultCurrentStudent;

                if (classForumResultCurrentStudent != null && classForumResultCurrentStudent.Status != EnumClassForumResultStatus.Draft)
                {
                    var classForumResultAllStudents = classForumResults
                        .Where(x => x.ClassForumId == classForum.Id &&
                                    x.Status != EnumClassForumResultStatus.Draft &&
                                    x.Id != classForumResultCurrentStudent.Id).ToList();
                    classForumByStudentModel.ClassForumResultAllStudents = classForumResultAllStudents;
                }
            }

            methodResult.Result = classForumByStudentModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
