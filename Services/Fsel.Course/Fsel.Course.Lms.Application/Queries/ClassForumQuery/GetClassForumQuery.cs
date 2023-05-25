// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ClassForumQuery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetClassForumQuery : IRequest<MethodResult<ClassForumModel>>
    {
        public Guid LessonResultId { get; set; }
    }

    public class GetClassForumQueryHandler : IRequestHandler<GetClassForumQuery, MethodResult<ClassForumModel>>
    {
        private readonly IClassForumRepository _classForumRepository;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;

        public GetClassForumQueryHandler(IClassForumRepository classForumRepository, IUserService userService, AuthContext authContext)
        {
            _classForumRepository = classForumRepository;
            _userService = userService;
            _authContext = authContext;
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
            var classForum = await _classForumRepository.Queryable
                .Include(x => x.ClassForumResults)
                .FirstOrDefaultAsync(x => x.Id == request.LessonResultId, cancellationToken);

            var classForumModel = new ClassForumModel
            {
                Id = classForum!.Id,
                CourseSkill = classForum.CourseSkill,
                GradingStyle = classForum.GradingStyle,
                IsActive = classForum.IsActive,
                MediaPost = classForum.MediaPost,
                TaggetTimeLimit = classForum.TaggetTimeLimit,
                TaggetWordLimit = classForum.TaggetWordLimit,
                ClassForumFiles = classForum.ClassForumFiles!.Select(x => new ClassForumFileModel
                {
                    Id = x.Id,
                    FilePath = x.FilePath,
                }).ToList(),
            };

            methodResult.Result = classForumModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
