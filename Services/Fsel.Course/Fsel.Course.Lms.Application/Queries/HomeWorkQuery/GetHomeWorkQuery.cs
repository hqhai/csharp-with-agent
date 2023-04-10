// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.HomeWorkQuery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetHomeWorkQuery : IRequest<MethodResult<HomeWorkModel>>
    {
        public Guid HomeWorkId { get; set; }
        /*        public Guid? LessonResultId { get; set; }*/
    }

    public class GetHomeWorkQueryHandler : IRequestHandler<GetHomeWorkQuery, MethodResult<HomeWorkModel>>
    {
        private readonly IMapper _mapper;
        private readonly IHomeWorkRepository _homeWorkRepository;
        private readonly QuestionTypeConverter _questionTypeConverter;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;

        public GetHomeWorkQueryHandler(IMapper mapper
            , IHomeWorkRepository homeWorkRepository
            , QuestionTypeConverter questionTypeConverter
            , AuthContext authContext

            , IUserService userService)
        {
            _mapper = mapper;
            _homeWorkRepository = homeWorkRepository;
            _questionTypeConverter = questionTypeConverter;
            _authContext = authContext;
            _userService = userService;
        }

        public async Task<MethodResult<HomeWorkModel>> Handle(GetHomeWorkQuery request, CancellationToken cancellationToken)
        {
            MethodResult<HomeWorkModel> methodResult = new MethodResult<HomeWorkModel>();

            var studentsResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId.ToString());
            if (studentsResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseClassStudentErrorCode.UserIdNotExist));
                return methodResult;
            }
            var studentId = studentsResult.Content!.Result!.Id;

            var homeWork = await _homeWorkRepository.Queryable
                                    .Include(x => x.HomeWorkQuestions)
                                    .ThenInclude(x => x.Question)
                                    .Where(x => x.Id == request.HomeWorkId).FirstOrDefaultAsync(cancellationToken: cancellationToken);
            if (homeWork == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumHomeWorkErrorCode.HomeWorkNotExist), nameof(request.HomeWorkId), request?.HomeWorkId);
                return methodResult;
            }
            var homeWorkModel = new HomeWorkModel
            {
                Id = homeWork.Id,
                Name = homeWork.Name,
                MediaPost = homeWork.MediaPost,
                Code = homeWork.Code,
                Questions = homeWork.HomeWorkQuestions.Where(x => x.Question != null).Select(x => x.Question).Select(n => new QuestionModel
                {
                    Id = n!.Id,
                    CorrectTotal = n!.CorrectTotal,
                    Ungraded = n!.Ungraded,
                    Explanation = n!.Explanation,
                    Config = n!.Config,
                    QuestionType = n!.QuestionType,
                }).ToList(),
            };
            methodResult.Result = homeWorkModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
