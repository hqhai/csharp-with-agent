// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ProgressQuery
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Repositories;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetUnitByClassForumDetailQuery : IRequest<MethodResult<IList<ClassForumAIModel>>>
    {
        public Guid LessonResultId { get; set; }
        public Guid ClassForumId { get; set; }
    }

    public class GetUnitByClassForumDetailQueryHandler : IRequestHandler<GetUnitByClassForumDetailQuery, MethodResult<IList<ClassForumAIModel>>>
    {
        private readonly AuthContext _authContext;
        private readonly IMapper _mapper;
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IClassForumRepository _classForumRepository;
        private readonly IUserService _userService;

        public GetUnitByClassForumDetailQueryHandler(AuthContext authContext
            , IMapper mapper
            , IClassForumResultRepository classForumResultRepository
            , ILessonResultRepository lessonResultRepository
            , IUserService userService
            , IClassForumRepository classForumRepository)
        {
            _authContext = authContext;
            _mapper = mapper;
            _classForumResultRepository = classForumResultRepository;
            _classForumRepository = classForumRepository;
            _lessonResultRepository = lessonResultRepository;
            _userService = userService;
        }

        public async Task<MethodResult<IList<ClassForumAIModel>>> Handle(GetUnitByClassForumDetailQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<ClassForumAIModel>> methodResult = new MethodResult<IList<ClassForumAIModel>>();
            var studentResult = await _userService.GetStudentByUserIdWithCacheAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentResult));
                return methodResult;
            }
            var student = studentResult?.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }
            var studentId = student.Id;
            var lessonResult = await _lessonResultRepository.GetByIdAsync(request.LessonResultId);
            if (lessonResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(lessonResult));
                return methodResult;
            }

            var classForum = await _classForumRepository.Queryable.Include(x => x.ClassForumResults.Where(x => x.LessonResultId == lessonResult.Id && x.StudentId == studentId))
                                                     .FirstOrDefaultAsync(x => x.Id == request.ClassForumId, cancellationToken);
            if (classForum == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(classForum));
                return methodResult;
            }
            var classForumResult = classForum.ClassForumResults.Where(x => x.StudentId == studentId).FirstOrDefault();
            if (classForumResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(classForumResult));
                return methodResult;
            }
            if (string.IsNullOrEmpty(classForumResult.GradingAlFeedback))
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            methodResult.Result = ConvertHelper.Deserialize<List<ClassForumAIModel>>(classForumResult.GradingAlFeedback);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
