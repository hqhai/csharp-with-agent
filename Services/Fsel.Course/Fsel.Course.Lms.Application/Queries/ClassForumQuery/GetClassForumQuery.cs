// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ClassForumQuery
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetClassForumQuery : IRequest<MethodResult<ClassForumModel>>
    {
        public Guid LessonId { get; set; }
        public Guid? LessonResultId { get; set; }
    }

    public class GetClassForumQueryHandler : IRequestHandler<GetClassForumQuery, MethodResult<ClassForumModel>>
    {
        private readonly IClassForumRepository _classForumRepository;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;
        private readonly IMapper _mapper;
        private readonly ILessonRepository _lessonRepository;

        public GetClassForumQueryHandler(IClassForumRepository classForumRepository, IUserService userService, AuthContext authContext, IMapper mapper, ILessonRepository lessonRepository)
        {
            _classForumRepository = classForumRepository;
            _userService = userService;
            _authContext = authContext;
            _mapper = mapper;
            _lessonRepository = lessonRepository;
        }

        public async Task<MethodResult<ClassForumModel>> Handle(GetClassForumQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ClassForumModel> methodResult = new MethodResult<ClassForumModel>();

            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            var student = studentResult?.Content?.Result;
            if (studentResult == null || student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.StudentNull));
                return methodResult;
            }
            var isLesson = await _lessonRepository.AnyAsync(request.LessonId);
            if (!isLesson)
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.LessonsNotExist));
                return methodResult;
            }

            var classForum = await _classForumRepository.Queryable
                .Include(x => x.ClassForumResults)
                .ThenInclude(x => x.ClassForumScores)
                .Include(x => x.ClassForumFiles)
                .Where(x => x.ClassForumResults == null || x.ClassForumResults.Any(x => x.Status == EnumClassForumResultStatus.PendingForGrading
                                                        || x.Status == EnumClassForumResultStatus.Graded
                                                        || x.Status != EnumClassForumResultStatus.Draft))
                .FirstOrDefaultAsync(x => x.LessonId == request.LessonId && x.ClassForumResults.Select(x => x.LessonResultId).Contains(request.LessonResultId ?? default), cancellationToken);

            methodResult.Result = _mapper.Map<ClassForumModel>(classForum);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
