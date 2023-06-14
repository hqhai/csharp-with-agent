// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.HomeWorkQuery
{
    using System;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetListHomeworkQuery : IRequest<MethodResult<IList<LessonHomeWorkResultModel>>>
    {
        public Guid LessonResultId { get; set; }
    }

    public class GetListHomeworkQueryHandler : IRequestHandler<GetListHomeworkQuery, MethodResult<IList<LessonHomeWorkResultModel>>>
    {
        private readonly IUserService _userService;
        private readonly IHomeWorkResultRepository _homeWorkResultRepository;
        private readonly IMapper _mapper;
        private readonly AuthContext _authContext;

        public GetListHomeworkQueryHandler(AuthContext authContext,
            IUserService userService,
            IHomeWorkResultRepository homeWorkResultRepository,
            IMapper mapper
            )
        {
            _authContext = authContext;
            _userService = userService;
            _homeWorkResultRepository = homeWorkResultRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<LessonHomeWorkResultModel>>> Handle(GetListHomeworkQuery request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<IList<LessonHomeWorkResultModel>>();

            var student = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!student.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumHomeWorkErrorCode.UserNotExist));
                return methodResult;
            }
            var studentId = student?.Content?.Result?.Id;

            var lessonSkillScoreQuery = _homeWorkResultRepository.Queryable
                                        .Include(x => x.HomeWork)
                                        .ThenInclude(x => x!.HomeWorkQuestions.Where(x => !x.IsDeleted))
                                        .ThenInclude(x => x.Question)
                                        .Include(x => x.HomeWorkAnswers.Where(x => !x.IsDeleted))
                                        .Where(x => x.HomeWork != null && x.LessonResultId == request.LessonResultId)
                                        .Select(h => new LessonHomeWorkResultModel
                                        {
                                            Id = h.HomeWork!.Id,
                                            Code = h.HomeWork.Code,
                                            Name = h.HomeWork.Name,
                                            CourseSkill = h.HomeWork.CourseSkill,
                                            CourseLevel = h.HomeWork.CourseLevel,
                                            QuestionTotal = h.HomeWork.HomeWorkQuestions.Select(x => x.Question).Count(),
                                            QuestionCompleted = h.HomeWorkAnswers.Count(),
                                            HomeWorkResult = _mapper.Map<HomeWorkResultModel>(h)
                                        });

            methodResult.Result = await lessonSkillScoreQuery.ToListAsync(cancellationToken);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
