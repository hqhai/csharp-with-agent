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
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;
        private readonly IMapper _mapper;
        private readonly ILessonRepository _lessonRepository;

        public GetClassForumQueryHandler(IClassForumRepository classForumRepository, IClassForumResultRepository classForumResultRepository, IUserService userService, AuthContext authContext, IMapper mapper, ILessonRepository lessonRepository)
        {
            _classForumRepository = classForumRepository;
            _classForumResultRepository = classForumResultRepository;
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
                .Include(x => x.ClassForumFiles)
                .FirstOrDefaultAsync(x => x.LessonId == request.LessonId, cancellationToken);

            if (classForum == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassForumErrorCode.ClassForumNotExist));
                return methodResult;
            }

            var classForumResult = await _classForumResultRepository.Queryable
                .Include(x => x.ClassForumResultFiles)
                .Include(x => x.ClassForumScores)
                .FirstOrDefaultAsync(x => x.ClassForumId == classForum.Id && x.LessonResultId == request.LessonResultId, cancellationToken);

            var classForumModel = _mapper.Map<ClassForumModel>(classForum);
            classForumModel.ClassForumResultCurrentStudent = _mapper.Map<ClassForumResultModel>(classForumResult);

            if (classForumResult != null && classForumResult.Status != EnumClassForumResultStatus.Draft)
            {
                var classForumResults = await _classForumResultRepository.Queryable
                    .Include(x => x.ClassForumResultFiles)
                    .Include(x => x.ClassForumScores)
                    .Where(x => x.ClassForumId == classForum.Id && x.Id != classForumResult.Id)
                    .ToListAsync(cancellationToken);

                classForumModel.ClassForumResultAllStudents = _mapper.Map<IList<ClassForumResultModel>>(classForumResults);
            }

            methodResult.Result = classForumModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
