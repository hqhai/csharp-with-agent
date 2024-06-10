// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ProgressQuery
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetUnitByClassForumDetailQuery : IRequest<MethodResult<IList<ClassForumScoreModel>>>
    {
        public Guid LessonResultId { get; set; }
        public Guid ClassForumId { get; set; }
    }

    public class GetUnitByClassForumDetailQueryHandler : IRequestHandler<GetUnitByClassForumDetailQuery, MethodResult<IList<ClassForumScoreModel>>>
    {
        private readonly AuthContext _authContext;
        private readonly IMapper _mapper;
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IUserService _userService;

        public GetUnitByClassForumDetailQueryHandler(AuthContext authContext
            , IMapper mapper
            , IClassForumResultRepository classForumResultRepository
            , ILessonResultRepository lessonResultRepository
            , IUserService userService)
        {
            _authContext = authContext;
            _mapper = mapper;
            _classForumResultRepository = classForumResultRepository;
            _lessonResultRepository = lessonResultRepository;
            _userService = userService;
        }

        public async Task<MethodResult<IList<ClassForumScoreModel>>> Handle(GetUnitByClassForumDetailQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<ClassForumScoreModel>> methodResult = new MethodResult<IList<ClassForumScoreModel>>();
            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
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
            var lessonResult = await _lessonResultRepository.GetByIdAsync(request.LessonResultId);
            if (lessonResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(lessonResult));
                return methodResult;
            }

            var classForumResult = await _classForumResultRepository.Queryable.Include(x => x.ClassForumScores).FirstOrDefaultAsync(x => x.LessonResultId == lessonResult.Id && x.ClassForumId == request.ClassForumId, cancellationToken);
            if (classForumResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(classForumResult));
                return methodResult;
            }
            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = _mapper.Map<IList<ClassForumScoreModel>>(classForumResult.ClassForumScores.OrderBy(x => x.Criteria));
            return methodResult;
        }
    }
}
